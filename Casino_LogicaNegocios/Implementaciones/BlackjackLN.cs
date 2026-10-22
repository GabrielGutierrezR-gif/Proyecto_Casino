using Casino_Dominio.Clases;
using Casino_Dominio.Entidades;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_LogicaNegocios.Blackjack;
using Casino_Utilitarios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Casino_LogicaNegocios.Implementaciones
{
    // Lógica del Blackjack multijugador 
    // Es SINGLETON: una sola instancia guarda en memoria todas las mesas activas.
    public class BlackjackLN : IBlackjackLN
    {
        // Mesas activas: IdSala → mesa
        private readonly ConcurrentDictionary<int, MesaBlackjack> _mesas = new();

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly INotificadorBlackjack _notificador;
        private readonly ILogger<BlackjackLN> _logger;

        public BlackjackLN(
            IServiceScopeFactory scopeFactory,
            INotificadorBlackjack notificador,
            ILogger<BlackjackLN> logger)
        {
            _scopeFactory = scopeFactory;
            _notificador = notificador;
            _logger = logger;
        }

       
        // ENTRAR A LA MESA
   
        public async Task<Respuesta<clsEstadoMesa>> EntrarAsync(int idSala, int idUsuario)
        {
            var mesa = await ObtenerOCrearMesaAsync(idSala);
            if (mesa == null)
                return Respuesta<clsEstadoMesa>.Fail("La sala no existe o ya fue cerrada.");

            await mesa.Candado.WaitAsync();
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                if (mesa.Cerrada)
                    return Respuesta<clsEstadoMesa>.Fail("La sala ya fue cerrada.");

                var jugador = mesa.Buscar(idUsuario);

                if (jugador == null)
                {
                    // Solo pueden sentarse los que se unieron a la sala 
                    var miembro = await uow.TJugadoresBlackjack
                        .ContarAsync(x => x.IdSala == idSala && x.IdUsuario == idUsuario);

                    if (miembro.Data == 0)
                        return Respuesta<clsEstadoMesa>.Fail("Primero debes unirte a la sala.");

                    var usuario = await uow.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);
                    jugador = new JugadorMesa(idUsuario, usuario.Data?.NombreUsuario ?? "Jugador");
                    mesa.Jugadores.Add(jugador);
                }

                // Si había salido o se le cayó la conexión, vuelve 
                jugador.Conectado = true;
                jugador.SalirAlTerminar = false;

                var estado = CrearEstado(mesa);
                await _notificador.EnviarMensajeAsync(idSala, $"{jugador.Nombre} entró a la mesa.");
                await _notificador.EnviarEstadoAsync(idSala, estado);

                return Respuesta<clsEstadoMesa>.Ok(estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al entrar a la mesa {IdSala}", idSala);
                return Respuesta<clsEstadoMesa>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

    
        // SALIR DE LA SALA
       
        public async Task<Respuesta<bool>> SalirAsync(int idSala, int idUsuario)
        {
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var uow = sp.GetRequiredService<IUnidadTrabajoEF>();

            // Si la mesa no está cargada en memoria, solo se sale de la sala en la BD
            if (!_mesas.TryGetValue(idSala, out var mesa))
                return await sp.GetRequiredService<ISalaLN>().SalirAsync(idUsuario, idSala);

            await mesa.Candado.WaitAsync();
            try
            {
                var jugador = mesa.Buscar(idUsuario);

                if (jugador == null)
                    return await sp.GetRequiredService<ISalaLN>().SalirAsync(idUsuario, idSala);

                // a) Está en plena ronda: sus manos se plantan y sale cuando termine
                if (mesa.Fase == FasesMesa.Jugando && jugador.Manos.Count > 0)
                {
                    bool eraSuTurno = mesa.TurnoJugador >= 0 && mesa.Jugadores[mesa.TurnoJugador] == jugador;

                    foreach (var mano in jugador.Manos.Where(m => !m.Terminada))
                    {
                        mano.Estado = EstadosMano.Plantado;
                        mano.UltimaAccion = "Abandono";
                    }

                    jugador.SalirAlTerminar = true;
                    jugador.Conectado = false;
                    await _notificador.EnviarMensajeAsync(idSala, $"{jugador.Nombre} abandonó la mesa.");

                    if (eraSuTurno)
                        await SiguienteTurnoAsync(mesa, sp);

                    if (!mesa.Cerrada)
                        await _notificador.EnviarEstadoAsync(idSala, CrearEstado(mesa));

                    return Respuesta<bool>.Ok(true);
                }

                // b) Estaba en fase de apuestas y ya había apostado: se le devuelven las fichas
                if (jugador.Manos.Count > 0)
                    await DevolverApuestasAsync(uow, jugador);

                await QuitarJugadorAsync(mesa, jugador, sp);

                if (!mesa.Cerrada)
                    await _notificador.EnviarEstadoAsync(idSala, CrearEstado(mesa));

                return Respuesta<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error al salir de la mesa {IdSala}", idSala);
                return Respuesta<bool>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

        // SE CAYÓ LA CONEXIÓN (puede volver con EntrarAsync)
       
        public async Task DesconectarAsync(int idUsuario)
        {
            foreach (var mesa in _mesas.Values)
            {
                await mesa.Candado.WaitAsync();
                using var scope = _scopeFactory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

                try
                {
                    var jugador = mesa.Buscar(idUsuario);
                    if (jugador == null || !jugador.Conectado) continue;

                    jugador.Conectado = false;
                    await _notificador.EnviarMensajeAsync(mesa.IdSala, $"{jugador.Nombre} se desconectó.");

                    // Si solo faltaba él por apostar, arranca la ronda con los demás
                    if (mesa.Fase == FasesMesa.Apuestas && mesa.TodosApostaron)
                        await IniciarRondaInternaAsync(mesa, scope.ServiceProvider);

                    await _notificador.EnviarEstadoAsync(mesa.IdSala, CrearEstado(mesa));
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    _logger.LogError(ex, "Error al desconectar al usuario {IdUsuario}", idUsuario);
                }
                finally
                {
                    mesa.Candado.Release();
                }
            }
        }

        
        // APOSTAR 
        
        public async Task<Respuesta<bool>> ApostarAsync(int idSala, int idUsuario, decimal monto)
        {
            var mesa = MesaActiva(idSala);
            if (mesa == null)
                return Respuesta<bool>.Fail("Primero entra a la mesa.");

            await mesa.Candado.WaitAsync();
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var uow = sp.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                var jugador = mesa.Buscar(idUsuario);
                if (jugador == null)
                    return Respuesta<bool>.Fail("No estás en esta mesa.");

                if (mesa.Fase != FasesMesa.Apuestas)
                    return Respuesta<bool>.Fail("La ronda ya comenzó. Espera a la siguiente.");

                if (jugador.Manos.Count > 0)
                    return Respuesta<bool>.Fail("Ya apostaste en esta ronda.");

                // Límites desde la tabla Configuracion (RG4)
                int minimo = await ObtenerConfiguracionEnteroAsync(uow, "ApuestaMinimaBlackjack", 10);
                int maximo = await ObtenerConfiguracionEnteroAsync(uow, "ApuestaMaximaBlackjack", 1000);

                if (monto < minimo || monto > maximo)
                    return Respuesta<bool>.Fail($"La apuesta debe estar entre {minimo} y {maximo} fichas.");

                // TRANSACCIÓN: descontar fichas + crear ronda (si es la primera apuesta) + guardar apuesta
                uow.EmpezarTransaccion();

                var (debitoOk, nuevoSaldo) = await DebitarAsync(uow, idUsuario, monto);
                if (!debitoOk)
                {
                    uow.Rollback();
                    return Respuesta<bool>.Fail($"Saldo insuficiente. Tienes {nuevoSaldo} fichas.");
                }

                int idRonda;
                bool rondaNueva = mesa.IdRondaActual == null;

                if (rondaNueva)
                {
                    var ronda = new RondasBlackjack
                    {
                        IdSala = idSala,
                        NumeroRonda = mesa.NumeroRonda + 1,
                        Estado = "Apuestas",
                        FechaInicio = DateTime.Now
                    };

                    var insertRonda = await uow.TRondasBlackjack.InsertarAsync(ronda);
                    if (!insertRonda.Success)
                    {
                        uow.Rollback();
                        return Respuesta<bool>.Fail(insertRonda.Error);
                    }

                    idRonda = ronda.IdRonda;
                }
                else
                {
                    idRonda = mesa.IdRondaActual!.Value;
                }

                var apuesta = new ApuestasBlackjack
                {
                    IdRonda = idRonda,
                    IdUsuario = idUsuario,
                    Apuesta = monto,
                    Ganancia = 0
                };

                var insertApuesta = await uow.TApuestasBlackjack.InsertarAsync(apuesta);
                if (!insertApuesta.Success)
                {
                    uow.Rollback();
                    return Respuesta<bool>.Fail(insertApuesta.Error);
                }

                uow.CompletarTran();

                // Actualizar la mesa en memoria
                if (rondaNueva)
                {
                    mesa.IdRondaActual = idRonda;
                    mesa.NumeroRonda++;
                }

                jugador.Manos.Add(new Mano { Apuesta = monto, IdApuestaBD = apuesta.IdApuesta });

                await _notificador.EnviarSaldoAsync(idUsuario, nuevoSaldo);
                await _notificador.EnviarMensajeAsync(idSala, $"{jugador.Nombre} apostó {monto} fichas.");

                // Si ya apostaron todos los conectados, la ronda arranca sola
                if (mesa.TodosApostaron)
                    await IniciarRondaInternaAsync(mesa, sp);

                await _notificador.EnviarEstadoAsync(idSala, CrearEstado(mesa));
                return Respuesta<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error al apostar en la mesa {IdSala}", idSala);
                return Respuesta<bool>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

        
        // INICIAR RONDA MANUALMENTE (sin esperar a los que no apostaron)
        
        public async Task<Respuesta<bool>> IniciarRondaAsync(int idSala, int idUsuario)
        {
            var mesa = MesaActiva(idSala);
            if (mesa == null)
                return Respuesta<bool>.Fail("Primero entra a la mesa.");

            await mesa.Candado.WaitAsync();
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                if (mesa.Buscar(idUsuario) == null)
                    return Respuesta<bool>.Fail("No estás en esta mesa.");

                if (mesa.Fase != FasesMesa.Apuestas)
                    return Respuesta<bool>.Fail("La ronda ya está en curso.");

                if (!mesa.Jugadores.Any(j => j.Manos.Count > 0))
                    return Respuesta<bool>.Fail("Nadie ha apostado todavía.");

                await IniciarRondaInternaAsync(mesa, scope.ServiceProvider);
                await _notificador.EnviarEstadoAsync(idSala, CrearEstado(mesa));

                return Respuesta<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error al iniciar ronda en la mesa {IdSala}", idSala);
                return Respuesta<bool>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

       
        // ACCIONES DEL JUGADOR: Pedir, Plantarse, Doblar, Dividir 
      
        public async Task<Respuesta<bool>> AccionAsync(int idSala, int idUsuario, string accion)
        {
            var mesa = MesaActiva(idSala);
            if (mesa == null)
                return Respuesta<bool>.Fail("Primero entra a la mesa.");

            await mesa.Candado.WaitAsync();
            using var scope = _scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var uow = sp.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                if (mesa.Fase != FasesMesa.Jugando || mesa.TurnoJugador < 0)
                    return Respuesta<bool>.Fail("No hay una ronda en curso.");

                var jugador = mesa.Jugadores[mesa.TurnoJugador];
                if (jugador.IdUsuario != idUsuario)
                    return Respuesta<bool>.Fail("No es tu turno.");

                var mano = mesa.Jugadores[mesa.TurnoJugador].Manos[mesa.TurnoMano];
                string? error;

                switch (accion)
                {
                    case "Pedir":
                        mano.Cartas.Add(mesa.Mazo.Sacar());
                        mano.UltimaAccion = "Pedir";
                        ActualizarTrasCarta(mano);
                        error = null;
                        break;

                    case "Plantarse":
                        mano.Estado = EstadosMano.Plantado;
                        mano.UltimaAccion = "Plantarse";
                        error = null;
                        break;

                    case "Doblar":
                        error = await DoblarAsync(mesa, jugador, mano, uow);
                        break;

                    case "Dividir":
                        error = await DividirAsync(mesa, jugador, mano, uow);
                        break;

                    default:
                        error = "Acción no válida. Use Pedir, Plantarse, Doblar o Dividir.";
                        break;
                }

                if (error != null)
                    return Respuesta<bool>.Fail(error);

                await _notificador.EnviarMensajeAsync(idSala,
                    $"{jugador.Nombre}: {accion} ({mano.Puntuacion} puntos).");

                // Si la mano terminó pasa al siguiente; si no, reinicia su tiempo
                await SiguienteTurnoAsync(mesa, sp);
                await _notificador.EnviarEstadoAsync(idSala, CrearEstado(mesa));

                return Respuesta<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error en la acción {Accion} de la mesa {IdSala}", accion, idSala);
                return Respuesta<bool>.Fail(ex.GetBaseException().Message);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

        // Doblar: duplica la apuesta, recibe UNA carta y se planta 
        private async Task<string?> DoblarAsync(MesaBlackjack mesa, JugadorMesa jugador, Mano mano, IUnidadTrabajoEF uow)
        {
            if (!mano.PuedeDoblar)
                return "Solo puedes doblar con tus dos primeras cartas.";

            uow.EmpezarTransaccion();

            var (debitoOk, nuevoSaldo) = await DebitarAsync(uow, jugador.IdUsuario, mano.Apuesta);
            if (!debitoOk)
            {
                uow.Rollback();
                return "Saldo insuficiente para doblar.";
            }

            int idApuesta = mano.IdApuestaBD;
            var fila = await uow.TApuestasBlackjack.ObtenerEntidadAsync(x => x.IdApuesta == idApuesta);
            if (!fila.Success || fila.Data == null)
            {
                uow.Rollback();
                return "No se encontró la apuesta.";
            }

            fila.Data.Apuesta = mano.Apuesta * 2;
            await uow.TApuestasBlackjack.ModificarAsync(fila.Data);

            uow.CompletarTran();

            mano.Apuesta *= 2;
            mano.Cartas.Add(mesa.Mazo.Sacar());
            mano.UltimaAccion = "Doblar";
            mano.Estado = mano.Puntuacion > 21 ? EstadosMano.Pasado : EstadosMano.Plantado;

            await _notificador.EnviarSaldoAsync(jugador.IdUsuario, nuevoSaldo);
            return null;
        }

        //  Dividir: dos cartas iguales se separan en dos manos
        private async Task<string?> DividirAsync(MesaBlackjack mesa, JugadorMesa jugador, Mano mano, IUnidadTrabajoEF uow)
        {
            if (!mano.PuedeDividir)
                return "Solo puedes dividir con dos cartas del mismo valor.";

            if (jugador.Manos.Count > 1)
                return "Solo se permite dividir una vez por ronda.";

            uow.EmpezarTransaccion();

            // La segunda mano lleva una apuesta igual a la primera
            var (debitoOk, nuevoSaldo) = await DebitarAsync(uow, jugador.IdUsuario, mano.Apuesta);
            if (!debitoOk)
            {
                uow.Rollback();
                return "Saldo insuficiente para dividir.";
            }

            var filaNueva = new ApuestasBlackjack
            {
                IdRonda = mesa.IdRondaActual!.Value,
                IdUsuario = jugador.IdUsuario,
                Apuesta = mano.Apuesta,
                Accion = "Dividir",
                Ganancia = 0
            };

            var insert = await uow.TApuestasBlackjack.InsertarAsync(filaNueva);
            if (!insert.Success)
            {
                uow.Rollback();
                return insert.Error;
            }

            uow.CompletarTran();

            // Separar las cartas: cada mano se queda con una y recibe otra nueva
            var segundaCarta = mano.Cartas[1];
            mano.Cartas.RemoveAt(1);

            var manoNueva = new Mano
            {
                Apuesta = mano.Apuesta,
                IdApuestaBD = filaNueva.IdApuesta,
                EsDividida = true,
                UltimaAccion = "Dividir"
            };
            manoNueva.Cartas.Add(segundaCarta);

            mano.EsDividida = true;
            mano.UltimaAccion = "Dividir";

            mano.Cartas.Add(mesa.Mazo.Sacar());
            manoNueva.Cartas.Add(mesa.Mazo.Sacar());

            jugador.Manos.Insert(mesa.TurnoMano + 1, manoNueva);

            // Regla de casino: si se dividen Ases, cada mano recibe solo una carta
            bool sonAses = segundaCarta.EsAs;
            foreach (var m in new[] { mano, manoNueva })
            {
                if (sonAses) m.Estado = EstadosMano.Plantado;
                else ActualizarTrasCarta(m);
            }

            await _notificador.EnviarSaldoAsync(jugador.IdUsuario, nuevoSaldo);
            return null;
        }

        
        // FLUJO DE LA RONDA
        

        // Reparte las cartas y arranca los turnos
        private async Task IniciarRondaInternaAsync(MesaBlackjack mesa, IServiceProvider sp)
        {
            var uow = sp.GetRequiredService<IUnidadTrabajoEF>();

            mesa.SegundosPorTurno = await ObtenerConfiguracionEnteroAsync(uow, "SegundosPorTurno", 30);

            if (mesa.Mazo.NecesitaBarajar)
            {
                mesa.Mazo.Barajar();
                await _notificador.EnviarMensajeAsync(mesa.IdSala, "Se barajó el mazo.");
            }

            // Reparto como en el casino: una vuelta a todos + dealer, luego otra vuelta
            mesa.CartasDealer.Clear();
            var participantes = mesa.Jugadores.Where(j => j.Manos.Count > 0).ToList();

            for (int vuelta = 0; vuelta < 2; vuelta++)
            {
                foreach (var jugador in participantes)
                    jugador.Manos[0].Cartas.Add(mesa.Mazo.Sacar());

                mesa.CartasDealer.Add(mesa.Mazo.Sacar());
            }

            foreach (var jugador in participantes)
            {
                if (jugador.Manos[0].EsBlackjack)
                {
                    jugador.Manos[0].Estado = EstadosMano.Blackjack;
                    jugador.Manos[0].UltimaAccion = "Blackjack";
                }
            }

            mesa.Fase = FasesMesa.Jugando;

            // BD: la ronda pasa a EnCurso y la sala a Jugando
            int idRonda = mesa.IdRondaActual!.Value;
            var ronda = await uow.TRondasBlackjack.ObtenerEntidadAsync(x => x.IdRonda == idRonda);
            if (ronda.Success && ronda.Data != null)
            {
                ronda.Data.Estado = "EnCurso";
                ronda.Data.FechaInicio = DateTime.Now;
                await uow.TRondasBlackjack.ModificarAsync(ronda.Data);
            }

            await CambiarEstadoSalaAsync(uow, mesa.IdSala, "Jugando");
            await _notificador.EnviarMensajeAsync(mesa.IdSala, $"¡Ronda {mesa.NumeroRonda} iniciada!");

            // Si el dealer tiene Blackjack, la ronda se resuelve de una vez
            if (Mano.CalcularPuntos(mesa.CartasDealer) == 21)
            {
                await _notificador.EnviarMensajeAsync(mesa.IdSala, "¡El dealer tiene Blackjack!");
                await FinalizarRondaAsync(mesa, sp);
                return;
            }

            await SiguienteTurnoAsync(mesa, sp);
        }

        // Busca la siguiente mano que todavía no termina; si no hay, juega el dealer
        private async Task SiguienteTurnoAsync(MesaBlackjack mesa, IServiceProvider sp)
        {
            CancelarTemporizador(mesa);

            for (int i = 0; i < mesa.Jugadores.Count; i++)
            {
                var manos = mesa.Jugadores[i].Manos;
                for (int k = 0; k < manos.Count; k++)
                {
                    if (!manos[k].Terminada)
                    {
                        mesa.TurnoJugador = i;
                        mesa.TurnoMano = k;
                        IniciarTemporizador(mesa);
                        return;
                    }
                }
            }

            // Todos terminaron
            mesa.TurnoJugador = -1;
            mesa.TurnoMano = -1;
            await FinalizarRondaAsync(mesa, sp);
        }

        // Juega el dealer, compara, paga y guarda todo 
        private async Task FinalizarRondaAsync(MesaBlackjack mesa, IServiceProvider sp)
        {
            var uow = sp.GetRequiredService<IUnidadTrabajoEF>();

            CancelarTemporizador(mesa);
            mesa.TurnoJugador = -1;
            mesa.TurnoMano = -1;

            var todasLasManos = mesa.Jugadores
                .SelectMany(j => j.Manos.Select(m => (Jugador: j, Mano: m)))
                .ToList();

            // 1) Regla del dealer: pide con 16 o menos, se planta con 17 o más 
            //    Solo juega si queda alguien que no se pasó y no tiene Blackjack.
            bool dealerJuega = todasLasManos.Any(x => x.Mano.Estado != EstadosMano.Pasado && !x.Mano.EsBlackjack);

            if (dealerJuega)
            {
                while (Mano.CalcularPuntos(mesa.CartasDealer) <= 16)
                    mesa.CartasDealer.Add(mesa.Mazo.Sacar());
            }

            int puntosDealer = Mano.CalcularPuntos(mesa.CartasDealer);
            bool dealerBlackjack = mesa.CartasDealer.Count == 2 && puntosDealer == 21;

            var resultadoRonda = new clsResultadoRonda
            {
                NumeroRonda = mesa.NumeroRonda,
                CartasDealer = mesa.CartasDealer.Select(ACarta).ToList(),
                PuntosDealer = puntosDealer
            };

            // 2) Comparar cada mano contra el dealer
            var devolucionPorUsuario = new Dictionary<int, decimal>();

            uow.EmpezarTransaccion();

            foreach (var (jugador, mano) in todasLasManos)
            {
                int puntos = mano.Puntuacion;
                decimal devolver;

                if (mano.Estado == EstadosMano.Pasado)
                {
                    mano.Resultado = "Pierde";
                    mano.Ganancia = -mano.Apuesta;
                    devolver = 0;
                }
                else if (mano.EsBlackjack && !dealerBlackjack)
                {
                    mano.Resultado = "Blackjack";                // paga 3 a 2
                    mano.Ganancia = mano.Apuesta * 1.5m;
                    devolver = mano.Apuesta + mano.Ganancia;
                }
                else if (dealerBlackjack && !mano.EsBlackjack)
                {
                    mano.Resultado = "Pierde";
                    mano.Ganancia = -mano.Apuesta;
                    devolver = 0;
                }
                else if (puntosDealer > 21 || puntos > puntosDealer)
                {
                    mano.Resultado = "Gana";                     // paga 1 a 1
                    mano.Ganancia = mano.Apuesta;
                    devolver = mano.Apuesta * 2;
                }
                else if (puntos == puntosDealer)
                {
                    mano.Resultado = "Empate";                   // se devuelve la apuesta
                    mano.Ganancia = 0;
                    devolver = mano.Apuesta;
                }
                else
                {
                    mano.Resultado = "Pierde";
                    mano.Ganancia = -mano.Apuesta;
                    devolver = 0;
                }

                devolucionPorUsuario.TryGetValue(jugador.IdUsuario, out decimal acumulado);
                devolucionPorUsuario[jugador.IdUsuario] = acumulado + devolver;

                // Guardar el resultado de la mano en ApuestasBlackjack
                int idApuesta = mano.IdApuestaBD;
                var fila = await uow.TApuestasBlackjack.ObtenerEntidadAsync(x => x.IdApuesta == idApuesta);
                if (fila.Success && fila.Data != null)
                {
                    fila.Data.Accion = mano.UltimaAccion;
                    fila.Data.Puntuacion = puntos;
                    fila.Data.Resultado = mano.Resultado;
                    fila.Data.Ganancia = mano.Ganancia;
                    fila.Data.Apuesta = mano.Apuesta;
                    await uow.TApuestasBlackjack.ModificarAsync(fila.Data);
                }

                resultadoRonda.Resultados.Add(new clsResultadoMano
                {
                    IdUsuario = jugador.IdUsuario,
                    Nombre = jugador.Nombre,
                    Cartas = mano.Cartas.Select(ACarta).ToList(),
                    Puntuacion = puntos,
                    Apuesta = mano.Apuesta,
                    Resultado = mano.Resultado,
                    Ganancia = mano.Ganancia
                });
            }

            // 3) Pagar a los ganadores
            foreach (var (idUsuario, monto) in devolucionPorUsuario)
            {
                if (monto > 0)
                    await AcreditarAsync(uow, idUsuario, monto);
            }

            // 4) Cerrar la ronda en la BD
            if (mesa.IdRondaActual != null)
            {
                int idRonda = mesa.IdRondaActual.Value;
                var ronda = await uow.TRondasBlackjack.ObtenerEntidadAsync(x => x.IdRonda == idRonda);
                if (ronda.Success && ronda.Data != null)
                {
                    ronda.Data.Estado = "Finalizada";
                    ronda.Data.ResultadoDealer = puntosDealer;
                    ronda.Data.FechaFin = DateTime.Now;
                    await uow.TRondasBlackjack.ModificarAsync(ronda.Data);
                }
            }

            await CambiarEstadoSalaAsync(uow, mesa.IdSala, "Esperando");

            uow.CompletarTran();

            // 5) Avisar a todos el resultado y a cada uno su saldo (RB2)
            await _notificador.EnviarRondaFinalizadaAsync(mesa.IdSala, resultadoRonda);

            foreach (var idUsuario in devolucionPorUsuario.Keys)
            {
                var usuario = await uow.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);
                if (usuario.Success && usuario.Data != null)
                    await _notificador.EnviarSaldoAsync(idUsuario, usuario.Data.SaldoFichas);
            }

            // 6) Limpiar la mesa para la siguiente ronda
            foreach (var jugador in mesa.Jugadores)
                jugador.Manos.Clear();

            mesa.CartasDealer.Clear();
            mesa.IdRondaActual = null;
            mesa.Fase = FasesMesa.Apuestas;

            // 7) Sacar a los que abandonaron durante la ronda
            foreach (var jugador in mesa.Jugadores.Where(j => j.SalirAlTerminar).ToList())
                await QuitarJugadorAsync(mesa, jugador, sp);
        }

        
        // TEMPORIZADOR DE TURNO
       
        private void IniciarTemporizador(MesaBlackjack mesa)
        {
            var cts = new CancellationTokenSource();
            mesa.Temporizador = cts;

            int segundos = Math.Max(5, mesa.SegundosPorTurno);
            mesa.FinTurnoUtc = DateTime.UtcNow.AddSeconds(segundos);
            var token = cts.Token;

            // Corre en segundo plano: si no lo cancelan antes, se acaba el tiempo
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(segundos), token);
                    await TiempoAgotadoAsync(mesa, cts);
                }
                catch (OperationCanceledException) { }   // el jugador decidió a tiempo
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el temporizador de la mesa {IdSala}", mesa.IdSala);
                }
            });
        }

        private static void CancelarTemporizador(MesaBlackjack mesa)
        {
            mesa.Temporizador?.Cancel();
            mesa.Temporizador = null;
            mesa.FinTurnoUtc = null;
        }

        // Se acabó el tiempo: la mano se planta sola
        private async Task TiempoAgotadoAsync(MesaBlackjack mesa, CancellationTokenSource cts)
        {
            await mesa.Candado.WaitAsync();
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                // Comprobar que sigue siendo el mismo turno (no se movió mientras esperábamos)
                if (mesa.Cerrada || mesa.Temporizador != cts || mesa.Fase != FasesMesa.Jugando || mesa.TurnoJugador < 0)
                    return;

                var jugador = mesa.Jugadores[mesa.TurnoJugador];
                var mano = jugador.Manos[mesa.TurnoMano];

                mano.Estado = EstadosMano.Plantado;
                mano.UltimaAccion = "TiempoAgotado";

                await _notificador.EnviarMensajeAsync(mesa.IdSala,
                    $"Se acabó el tiempo de {jugador.Nombre}: se planta automáticamente.");

                await SiguienteTurnoAsync(mesa, scope.ServiceProvider);
                await _notificador.EnviarEstadoAsync(mesa.IdSala, CrearEstado(mesa));
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error al agotar el tiempo en la mesa {IdSala}", mesa.IdSala);
            }
            finally
            {
                mesa.Candado.Release();
            }
        }

        
        // AL ARRANCAR LA API: devolver fichas de rondas que quedaron a medias
       
        public async Task CancelarRondasPendientesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

            try
            {
                var rondas = await uow.TRondasBlackjack
                    .BuscarAsync(x => x.Estado == "Apuestas" || x.Estado == "EnCurso");

                foreach (var ronda in rondas.Data ?? Enumerable.Empty<RondasBlackjack>())
                {
                    uow.EmpezarTransaccion();

                    int idRonda = ronda.IdRonda;
                    var apuestas = await uow.TApuestasBlackjack
                        .BuscarAsync(x => x.IdRonda == idRonda && x.Resultado == null);

                    foreach (var apuesta in apuestas.Data ?? Enumerable.Empty<ApuestasBlackjack>())
                    {
                        await AcreditarAsync(uow, apuesta.IdUsuario, apuesta.Apuesta);
                        apuesta.Resultado = "Cancelada";
                        apuesta.Ganancia = 0;
                        await uow.TApuestasBlackjack.ModificarAsync(apuesta);
                    }

                    ronda.Estado = "Cancelada";
                    ronda.FechaFin = DateTime.Now;
                    await uow.TRondasBlackjack.ModificarAsync(ronda);

                    uow.CompletarTran();
                }

                // Las salas que se quedaron "Jugando" vuelven a "Esperando"
                var salas = await uow.TSalasBlackjack.BuscarAsync(x => x.Estado == "Jugando");
                foreach (var sala in salas.Data ?? Enumerable.Empty<SalasBlackjack>())
                {
                    sala.Estado = "Esperando";
                    await uow.TSalasBlackjack.ModificarAsync(sala);
                }

                _logger.LogInformation("Rondas pendientes canceladas: {Cantidad}", rondas.Data?.Count() ?? 0);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                _logger.LogError(ex, "Error al cancelar rondas pendientes.");
            }
        }

        
        // MÉTODOS DE APOYO
        

        private MesaBlackjack? MesaActiva(int idSala) =>
            _mesas.TryGetValue(idSala, out var mesa) && !mesa.Cerrada ? mesa : null;

        // Carga la mesa desde la BD la primera vez que alguien entra
        private async Task<MesaBlackjack?> ObtenerOCrearMesaAsync(int idSala)
        {
            if (_mesas.TryGetValue(idSala, out var existente) && !existente.Cerrada)
                return existente;

            using var scope = _scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnidadTrabajoEF>();

            var sala = await uow.TSalasBlackjack
                .ObtenerEntidadAsync(x => x.IdSala == idSala && x.Estado != "Cerrada");

            if (!sala.Success || sala.Data == null)
                return null;

            var nueva = new MesaBlackjack(sala.Data.IdSala, sala.Data.CodigoSala, sala.Data.NombreSala);
            nueva.NumeroRonda = (await uow.TRondasBlackjack.ContarAsync(x => x.IdSala == idSala)).Data ?? 0;

            _mesas[idSala] = nueva;
            return nueva;
        }

        // Saca al jugador de la mesa y de la sala en la BD 
        private async Task QuitarJugadorAsync(MesaBlackjack mesa, JugadorMesa jugador, IServiceProvider sp)
        {
            mesa.Jugadores.Remove(jugador);

            await sp.GetRequiredService<ISalaLN>().SalirAsync(jugador.IdUsuario, mesa.IdSala);
            await _notificador.EnviarMensajeAsync(mesa.IdSala, $"{jugador.Nombre} salió de la sala.");

            // Si ya no queda nadie sentado, la mesa se borra de memoria
            if (mesa.Jugadores.Count == 0)
            {
                mesa.Cerrada = true;
                CancelarTemporizador(mesa);
                _mesas.TryRemove(mesa.IdSala, out _);

                if (mesa.IdRondaActual != null)
                {
                    var uow = sp.GetRequiredService<IUnidadTrabajoEF>();
                    int idRonda = mesa.IdRondaActual.Value;
                    var ronda = await uow.TRondasBlackjack.ObtenerEntidadAsync(x => x.IdRonda == idRonda);
                    if (ronda.Success && ronda.Data != null)
                    {
                        ronda.Data.Estado = "Cancelada";
                        ronda.Data.FechaFin = DateTime.Now;
                        await uow.TRondasBlackjack.ModificarAsync(ronda.Data);
                    }
                }
            }
        }

        // Devuelve las fichas si sale en fase de apuestas
        private async Task DevolverApuestasAsync(IUnidadTrabajoEF uow, JugadorMesa jugador)
        {
            uow.EmpezarTransaccion();

            decimal total = 0;
            foreach (var mano in jugador.Manos)
            {
                int idApuesta = mano.IdApuestaBD;
                var fila = await uow.TApuestasBlackjack.ObtenerEntidadAsync(x => x.IdApuesta == idApuesta);
                if (fila.Success && fila.Data != null)
                    await uow.TApuestasBlackjack.EliminarAsync(fila.Data);

                total += mano.Apuesta;
            }

            var (_, saldo) = await AcreditarAsync(uow, jugador.IdUsuario, total);
            uow.CompletarTran();

            jugador.Manos.Clear();
            await _notificador.EnviarSaldoAsync(jugador.IdUsuario, saldo);
        }

        // Después de recibir una carta: ¿se pasó?, ¿llegó a 21?
        private static void ActualizarTrasCarta(Mano mano)
        {
            if (mano.Puntuacion > 21) mano.Estado = EstadosMano.Pasado;
            else if (mano.Puntuacion == 21) mano.Estado = EstadosMano.Plantado;
        }

        // Resta fichas si alcanza el saldo
        private static async Task<(bool ok, decimal saldo)> DebitarAsync(IUnidadTrabajoEF uow, int idUsuario, decimal monto)
        {
            var usuario = await uow.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);
            if (!usuario.Success || usuario.Data == null) return (false, 0);
            if (usuario.Data.SaldoFichas < monto) return (false, usuario.Data.SaldoFichas);

            usuario.Data.SaldoFichas -= monto;
            var modificar = await uow.TUsuario.ModificarAsync(usuario.Data);
            return (modificar.Success, usuario.Data.SaldoFichas);
        }

        // Suma fichas
        private static async Task<(bool ok, decimal saldo)> AcreditarAsync(IUnidadTrabajoEF uow, int idUsuario, decimal monto)
        {
            var usuario = await uow.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);
            if (!usuario.Success || usuario.Data == null) return (false, 0);

            usuario.Data.SaldoFichas += monto;
            var modificar = await uow.TUsuario.ModificarAsync(usuario.Data);
            return (modificar.Success, usuario.Data.SaldoFichas);
        }

        private static async Task CambiarEstadoSalaAsync(IUnidadTrabajoEF uow, int idSala, string estado)
        {
            var sala = await uow.TSalasBlackjack.ObtenerEntidadAsync(x => x.IdSala == idSala);
            if (sala.Success && sala.Data != null && sala.Data.Estado != "Cerrada")
            {
                sala.Data.Estado = estado;
                await uow.TSalasBlackjack.ModificarAsync(sala.Data);
            }
        }

        private static async Task<int> ObtenerConfiguracionEnteroAsync(IUnidadTrabajoEF uow, string clave, int porDefecto)
        {
            var config = await uow.TConfiguracion.ObtenerEntidadAsync(x => x.Clave == clave);
            if (config.Success && config.Data != null && int.TryParse(config.Data.Valor, out int valor))
                return valor;
            return porDefecto;
        }

        // Convierte la mesa en memoria a lo que ve la app.
        // La segunda carta del dealer va OCULTA mientras se juega.
        private static clsEstadoMesa CrearEstado(MesaBlackjack mesa)
        {
            bool ocultar = mesa.Fase == FasesMesa.Jugando && mesa.CartasDealer.Count >= 2;

            var cartasDealer = mesa.CartasDealer
                .Select((c, i) => ocultar && i == 1 ? new clsCarta { Oculta = true } : ACarta(c))
                .ToList();

            int? puntosDealer = mesa.CartasDealer.Count == 0
                ? null
                : ocultar
                    ? Mano.CalcularPuntos(mesa.CartasDealer.Take(1))
                    : Mano.CalcularPuntos(mesa.CartasDealer);

            var jugadores = mesa.Jugadores.Select((j, i) => new clsJugadorMesa
            {
                IdUsuario = j.IdUsuario,
                Nombre = j.Nombre,
                Conectado = j.Conectado,
                YaAposto = j.Manos.Count > 0,
                Manos = j.Manos.Select((m, k) =>
                {
                    bool esTurno = i == mesa.TurnoJugador && k == mesa.TurnoMano;
                    return new clsManoMesa
                    {
                        Cartas = m.Cartas.Select(ACarta).ToList(),
                        Puntuacion = m.Puntuacion,
                        Apuesta = m.Apuesta,
                        Estado = m.Estado,
                        EsTurno = esTurno,
                        PuedeDoblar = esTurno && m.PuedeDoblar,
                        PuedeDividir = esTurno && m.PuedeDividir && j.Manos.Count == 1
                    };
                }).ToList()
            }).ToList();

            return new clsEstadoMesa
            {
                IdSala = mesa.IdSala,
                CodigoSala = mesa.CodigoSala,
                NombreSala = mesa.NombreSala,
                Fase = mesa.Fase,
                NumeroRonda = mesa.NumeroRonda,
                CartasDealer = cartasDealer,
                PuntosDealer = puntosDealer,
                Jugadores = jugadores,
                TurnoIdUsuario = mesa.TurnoJugador >= 0 ? mesa.Jugadores[mesa.TurnoJugador].IdUsuario : null,
                FinTurnoUtc = mesa.FinTurnoUtc
            };
        }

        private static clsCarta ACarta(Carta c) => new() { Valor = c.Valor, Palo = c.Palo, Oculta = false };
    }
}