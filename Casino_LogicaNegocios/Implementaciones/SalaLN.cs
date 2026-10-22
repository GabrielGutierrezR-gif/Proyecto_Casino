using Casino_Dominio.Clases;
using Casino_Dominio.Entidades;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Casino_LogicaNegocios.Implementaciones
{
    public class SalaLN : ISalaLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<SalaLN> _logger;

        // Estados posibles de una sala
        private const string EstadoEsperando = "Esperando";
        private const string EstadoCerrada = "Cerrada";

        public SalaLN(IUnidadTrabajoEF unidadTrabajo, ILogger<SalaLN> logger)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
        }

        // ========== Lista de salas públicas activas (RB1) ==========
        public async Task<Respuesta<IEnumerable<clsSala>>> ListarPublicasAsync()
        {
            var resultado = new Respuesta<IEnumerable<clsSala>>();

            try
            {
                var salas = await _unidadDeTrabajo.TSalasBlackjack
                    .BuscarAsync(x => x.TipoSala == "Publica" && x.Estado != EstadoCerrada);

                if (!salas.Success)
                {
                    resultado.Success = false;
                    resultado.Error = salas.Error;
                    return resultado;
                }

                var lista = new List<clsSala>();
                foreach (var sala in salas.Data!.OrderBy(s => s.FechaCreacion))
                {
                    lista.Add(await ConstruirSalaAsync(sala));
                }

                resultado.Data = lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar salas públicas.");
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Detalle de una sala ==========
        public async Task<Respuesta<clsSala>> ObtenerSalaAsync(int idSala)
        {
            var resultado = new Respuesta<clsSala>();

            try
            {
                var sala = await _unidadDeTrabajo.TSalasBlackjack
                    .ObtenerEntidadAsync(x => x.IdSala == idSala && x.Estado != EstadoCerrada);

                if (!sala.Success || sala.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "La sala no existe o ya fue cerrada.";
                    return resultado;
                }

                resultado.Data = await ConstruirSalaAsync(sala.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sala {IdSala}", idSala);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== ¿En qué sala estoy? (para reconectar, RNF6) ==========
        public async Task<Respuesta<clsSala>> ObtenerMiSalaAsync(int idUsuario)
        {
            var resultado = new Respuesta<clsSala>();

            try
            {
                var sala = await BuscarSalaActivaDelUsuarioAsync(idUsuario);

                if (sala == null)
                {
                    resultado.Success = false;
                    resultado.Error = "No estás en ninguna sala.";
                    return resultado;
                }

                resultado.Data = await ConstruirSalaAsync(sala);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la sala del usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Crear sala (RB1 + RNF4) ==========
        public async Task<Respuesta<clsSala>> CrearAsync(int idUsuario, clsCrearSalaRequest datos)
        {
            var resultado = new Respuesta<clsSala>();

            try
            {
                // 1) Validaciones
                var nombre = datos.NombreSala?.Trim() ?? "";

                if (nombre.Length < 3 || nombre.Length > 50)
                {
                    resultado.Success = false;
                    resultado.Error = "El nombre de la sala debe tener entre 3 y 50 caracteres.";
                    return resultado;
                }

                if (datos.TipoSala != "Publica" && datos.TipoSala != "Privada")
                {
                    resultado.Success = false;
                    resultado.Error = "El tipo de sala debe ser 'Publica' o 'Privada'.";
                    return resultado;
                }

                // 2) ¿Ya está en otra sala?
                if (await BuscarSalaActivaDelUsuarioAsync(idUsuario) != null)
                {
                    resultado.Success = false;
                    resultado.Error = "Ya estás en una sala. Sal de ella antes de crear otra.";
                    return resultado;
                }

                // 3) ¿Se alcanzó el máximo de salas activas? (RNF4)
                int maxSalas = await ObtenerConfiguracionEnteroAsync("MaxSalasActivas", 5);
                var activas = await _unidadDeTrabajo.TSalasBlackjack.ContarAsync(x => x.Estado != EstadoCerrada);

                if (activas.Data >= maxSalas)
                {
                    resultado.Success = false;
                    resultado.Error = $"Se alcanzó el máximo de {maxSalas} salas activas. Intenta más tarde o únete a una existente.";
                    return resultado;
                }

                // 4) Código único
                string codigo = await GenerarCodigoUnicoAsync();

                // 5) TRANSACCIÓN: crear la sala + agregar al creador como jugador
                _unidadDeTrabajo.EmpezarTransaccion();

                var sala = new SalasBlackjack
                {
                    CodigoSala = codigo,
                    NombreSala = nombre,
                    TipoSala = datos.TipoSala,
                    Estado = EstadoEsperando,
                    FechaCreacion = DateTime.Now
                };

                var insertSala = await _unidadDeTrabajo.TSalasBlackjack.InsertarAsync(sala);
                if (!insertSala.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = insertSala.Error;
                    return resultado;
                }

                var jugador = new JugadoresBlackjack
                {
                    IdSala = sala.IdSala,
                    IdUsuario = idUsuario,
                    FechaUnion = DateTime.Now
                };

                var insertJugador = await _unidadDeTrabajo.TJugadoresBlackjack.InsertarAsync(jugador);
                if (!insertJugador.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = insertJugador.Error;
                    return resultado;
                }

                _unidadDeTrabajo.CompletarTran();

                resultado.Data = await ConstruirSalaAsync(sala);
            }
            catch (Exception ex)
            {
                _unidadDeTrabajo.Rollback();
                _logger.LogError(ex, "Error al crear sala. Usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Unirse con código (RB1) ==========
        public async Task<Respuesta<clsSala>> UnirseAsync(int idUsuario, clsUnirseSalaRequest datos)
        {
            var resultado = new Respuesta<clsSala>();

            try
            {
                var codigo = datos.CodigoSala?.Trim().ToUpper() ?? "";

                // 1) ¿Existe la sala y está abierta?
                var sala = await _unidadDeTrabajo.TSalasBlackjack
                    .ObtenerEntidadAsync(x => x.CodigoSala == codigo && x.Estado != EstadoCerrada);

                if (!sala.Success || sala.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "No existe una sala activa con ese código.";
                    return resultado;
                }

                // 2) ¿Ya está en alguna sala?
                var salaActual = await BuscarSalaActivaDelUsuarioAsync(idUsuario);

                if (salaActual != null && salaActual.IdSala == sala.Data.IdSala)
                {
                    // Ya estaba en ESTA sala → no es error, simplemente la devolvemos
                    resultado.Data = await ConstruirSalaAsync(sala.Data);
                    return resultado;
                }

                if (salaActual != null)
                {
                    resultado.Success = false;
                    resultado.Error = "Ya estás en otra sala. Sal de ella antes de unirte a una nueva.";
                    return resultado;
                }

                // 3) ¿Hay espacio? (máximo 4)
                int maxJugadores = await ObtenerConfiguracionEnteroAsync("MaxJugadoresPorSala", 4);
                var cantidad = await _unidadDeTrabajo.TJugadoresBlackjack
                    .ContarAsync(x => x.IdSala == sala.Data.IdSala);

                if (cantidad.Data >= maxJugadores)
                {
                    resultado.Success = false;
                    resultado.Error = $"La sala está llena (máximo {maxJugadores} jugadores).";
                    return resultado;
                }

                // 4) Agregar al jugador
                var jugador = new JugadoresBlackjack
                {
                    IdSala = sala.Data.IdSala,
                    IdUsuario = idUsuario,
                    FechaUnion = DateTime.Now
                };

                var insertJugador = await _unidadDeTrabajo.TJugadoresBlackjack.InsertarAsync(jugador);
                if (!insertJugador.Success)
                {
                    resultado.Success = false;
                    resultado.Error = insertJugador.Error;
                    return resultado;
                }

                resultado.Data = await ConstruirSalaAsync(sala.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al unirse a sala. Usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Salir de la sala (RNF4: si queda vacía se cierra) ==========
        public async Task<Respuesta<bool>> SalirAsync(int idUsuario, int idSala)
        {
            var resultado = new Respuesta<bool>();

            try
            {
                var jugador = await _unidadDeTrabajo.TJugadoresBlackjack
                    .ObtenerEntidadAsync(x => x.IdSala == idSala && x.IdUsuario == idUsuario);

                if (!jugador.Success || jugador.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "No estás en esta sala.";
                    return resultado;
                }

                _unidadDeTrabajo.EmpezarTransaccion();

                // 1) Quitar al jugador
                var eliminar = await _unidadDeTrabajo.TJugadoresBlackjack.EliminarAsync(jugador.Data);
                if (!eliminar.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = eliminar.Error;
                    return resultado;
                }

                // 2) Si ya no queda nadie, cerrar la sala para liberar el cupo
                var quedan = await _unidadDeTrabajo.TJugadoresBlackjack.ContarAsync(x => x.IdSala == idSala);

                if (quedan.Data == 0)
                {
                    var sala = await _unidadDeTrabajo.TSalasBlackjack.ObtenerEntidadAsync(x => x.IdSala == idSala);
                    if (sala.Success && sala.Data != null)
                    {
                        sala.Data.Estado = EstadoCerrada;
                        await _unidadDeTrabajo.TSalasBlackjack.ModificarAsync(sala.Data);
                    }
                }

                _unidadDeTrabajo.CompletarTran();
                resultado.Data = true;
            }
            catch (Exception ex)
            {
                _unidadDeTrabajo.Rollback();
                _logger.LogError(ex, "Error al salir de sala {IdSala}. Usuario {IdUsuario}", idSala, idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Métodos de apoyo ==========

        // Arma el clsSala con la cantidad de jugadores y sus nombres
        private async Task<clsSala> ConstruirSalaAsync(SalasBlackjack sala)
        {
            int maxJugadores = await ObtenerConfiguracionEnteroAsync("MaxJugadoresPorSala", 4);

            var jugadores = await _unidadDeTrabajo.TJugadoresBlackjack.BuscarAsync(x => x.IdSala == sala.IdSala);
            var idsUsuarios = jugadores.Data?.Select(j => j.IdUsuario).ToList() ?? new List<int>();

            var usuarios = await _unidadDeTrabajo.TUsuario.BuscarAsync(x => idsUsuarios.Contains(x.IdUsuario));
            var nombres = usuarios.Data?.Select(u => u.NombreUsuario).ToList() ?? new List<string>();

            return new clsSala
            {
                IdSala = sala.IdSala,
                CodigoSala = sala.CodigoSala,
                NombreSala = sala.NombreSala,
                TipoSala = sala.TipoSala,
                Estado = sala.Estado,
                CantidadJugadores = idsUsuarios.Count,
                MaxJugadores = maxJugadores,
                Jugadores = nombres,
                FechaCreacion = sala.FechaCreacion
            };
        }

        // Busca si el usuario ya está en alguna sala que no esté cerrada
        private async Task<SalasBlackjack?> BuscarSalaActivaDelUsuarioAsync(int idUsuario)
        {
            var membresias = await _unidadDeTrabajo.TJugadoresBlackjack.BuscarAsync(x => x.IdUsuario == idUsuario);
            var idsSalas = membresias.Data?.Select(m => m.IdSala).ToList() ?? new List<int>();

            if (idsSalas.Count == 0) return null;

            var salas = await _unidadDeTrabajo.TSalasBlackjack
                .BuscarAsync(x => idsSalas.Contains(x.IdSala) && x.Estado != EstadoCerrada);

            return salas.Data?.FirstOrDefault();
        }

        // Genera un código de 6 caracteres que no exista. Sin 0/O ni 1/I para que no se confundan.
        private async Task<string> GenerarCodigoUnicoAsync()
        {
            const string letras = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            string codigo;
            int? repetidos;

            do
            {
                var caracteres = new char[6];
                for (int i = 0; i < caracteres.Length; i++)
                {
                    caracteres[i] = letras[RandomNumberGenerator.GetInt32(letras.Length)];
                }
                codigo = new string(caracteres);

                repetidos = (await _unidadDeTrabajo.TSalasBlackjack.ContarAsync(x => x.CodigoSala == codigo)).Data;
            }
            while (repetidos > 0);

            return codigo;
        }

        // Lee un valor numérico de la tabla Configuracion (RG4)
        private async Task<int> ObtenerConfiguracionEnteroAsync(string clave, int porDefecto)
        {
            var config = await _unidadDeTrabajo.TConfiguracion.ObtenerEntidadAsync(x => x.Clave == clave);

            if (config.Success && config.Data != null && int.TryParse(config.Data.Valor, out int valor))
                return valor;

            return porDefecto;
        }
    }
}