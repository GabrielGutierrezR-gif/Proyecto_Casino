using Casino_Dominio.Clases;
using Casino_Dominio.Entidades;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Casino_LogicaNegocios.Implementaciones
{
    public class RuletaLN : IRuletaLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<RuletaLN> _logger;

        // Números rojos de la ruleta europea (los demás, excepto el 0, son negros)
        private static readonly HashSet<int> NumerosRojos = new()
        {
            1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
        };

        public RuletaLN(IUnidadTrabajoEF unidadTrabajo, ILogger<RuletaLN> logger)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
        }

        // ========== Girar la ruleta (RR2 + RR3) ==========
        public async Task<Respuesta<clsGiroResultado>> GirarAsync(int idUsuario, clsGiroRequest datos)
        {
            var resultado = new Respuesta<clsGiroResultado>();

            try
            {
                // 1) Validar las apuestas
                if (datos.Apuestas == null || datos.Apuestas.Count == 0)
                {
                    resultado.Success = false;
                    resultado.Error = "Debe realizar al menos una apuesta.";
                    return resultado;
                }

                foreach (var apuesta in datos.Apuestas)
                {
                    string? error = ValidarApuesta(apuesta);
                    if (error != null)
                    {
                        resultado.Success = false;
                        resultado.Error = error;
                        return resultado;
                    }
                }

                decimal totalApostado = datos.Apuestas.Sum(a => a.Monto);

                // 2) ¿Tiene saldo suficiente?
                var usuario = await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);

                if (!usuario.Success || usuario.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "Usuario no encontrado.";
                    return resultado;
                }

                if (usuario.Data.SaldoFichas < totalApostado)
                {
                    resultado.Success = false;
                    resultado.Error = $"Saldo insuficiente. Tienes {usuario.Data.SaldoFichas} fichas y apostaste {totalApostado}.";
                    return resultado;
                }

                // 3) Número ganador aleatorio generado en el SERVIDOR (RR2)
                int numeroGanador = RandomNumberGenerator.GetInt32(0, 37);   // de 0 a 36
                string colorGanador = ObtenerColor(numeroGanador);

                // 4) TRANSACCIÓN: partida + apuestas + saldo (RNF5)
                _unidadDeTrabajo.EmpezarTransaccion();

                var partida = new PartidasRuletum
                {
                    NumeroGanador = numeroGanador,
                    ColorGanador = colorGanador,
                    FechaPartida = DateTime.Now
                };

                var insertPartida = await _unidadDeTrabajo.TPartidasRuleta.InsertarAsync(partida);
                if (!insertPartida.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = insertPartida.Error;
                    return resultado;
                }

                // 5) Evaluar cada apuesta contra el número ganador (RR3)
                var detalle = new List<clsResultadoApuesta>();
                decimal totalDevuelto = 0;

                foreach (var apuesta in datos.Apuestas)
                {
                    bool gana = EsGanadora(apuesta, numeroGanador);
                    int pago = ObtenerPago(apuesta.TipoApuesta);

                    // Ganancia neta: si gana, el premio; si pierde, lo apostado en negativo
                    decimal ganancia = gana ? apuesta.Monto * pago : -apuesta.Monto;

                    // Si gana, se le devuelve lo apostado + el premio
                    if (gana) totalDevuelto += apuesta.Monto + (apuesta.Monto * pago);

                    var registro = new ApuestasRuletum
                    {
                        IdPartida = partida.IdPartida,
                        IdUsuario = idUsuario,
                        TipoApuesta = apuesta.TipoApuesta,
                        ValorApuesta = apuesta.ValorApuesta,
                        Monto = apuesta.Monto,
                        Resultado = gana ? "Gana" : "Pierde",
                        Ganancia = ganancia
                    };

                    var insertApuesta = await _unidadDeTrabajo.TApuestasRuleta.InsertarAsync(registro);
                    if (!insertApuesta.Success)
                    {
                        _unidadDeTrabajo.Rollback();
                        resultado.Success = false;
                        resultado.Error = insertApuesta.Error;
                        return resultado;
                    }

                    detalle.Add(new clsResultadoApuesta
                    {
                        TipoApuesta = apuesta.TipoApuesta,
                        ValorApuesta = apuesta.ValorApuesta,
                        Monto = apuesta.Monto,
                        Resultado = registro.Resultado,
                        Ganancia = ganancia
                    });
                }

                // 6) Actualizar saldo: se quita lo apostado y se suma lo ganado
                usuario.Data.SaldoFichas = usuario.Data.SaldoFichas - totalApostado + totalDevuelto;

                var updateUsuario = await _unidadDeTrabajo.TUsuario.ModificarAsync(usuario.Data);
                if (!updateUsuario.Success)
                {
                    _unidadDeTrabajo.Rollback();
                    resultado.Success = false;
                    resultado.Error = updateUsuario.Error;
                    return resultado;
                }

                _unidadDeTrabajo.CompletarTran();

                resultado.Data = new clsGiroResultado
                {
                    IdPartida = partida.IdPartida,
                    NumeroGanador = numeroGanador,
                    ColorGanador = colorGanador,
                    TotalApostado = totalApostado,
                    TotalGanado = totalDevuelto,
                    NuevoSaldo = usuario.Data.SaldoFichas,
                    Detalle = detalle
                };
            }
            catch (Exception ex)
            {
                _unidadDeTrabajo.Rollback();
                _logger.LogError(ex, "Error al girar la ruleta. Usuario {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Reglas de la ruleta ==========

        // Revisa que la apuesta tenga un tipo y valor válidos. Devuelve el error o null si está bien.
        private static string? ValidarApuesta(clsApuestaRuleta a)
        {
            if (a.Monto <= 0)
                return "El monto de cada apuesta debe ser mayor a 0.";

            switch (a.TipoApuesta)
            {
                case "Numero":
                    if (!int.TryParse(a.ValorApuesta, out int n) || n < 0 || n > 36)
                        return "En apuestas a número, el valor debe ser de 0 a 36.";
                    break;

                case "Color":
                    if (a.ValorApuesta != "Rojo" && a.ValorApuesta != "Negro")
                        return "En apuestas a color, el valor debe ser 'Rojo' o 'Negro'.";
                    break;

                case "ParImpar":
                    if (a.ValorApuesta != "Par" && a.ValorApuesta != "Impar")
                        return "En apuestas Par/Impar, el valor debe ser 'Par' o 'Impar'.";
                    break;

                case "Docena":
                    if (a.ValorApuesta != "1" && a.ValorApuesta != "2" && a.ValorApuesta != "3")
                        return "En apuestas a docena, el valor debe ser '1', '2' o '3'.";
                    break;

                default:
                    return $"Tipo de apuesta no válido: '{a.TipoApuesta}'. Use Numero, Color, ParImpar o Docena.";
            }

            return null;
        }

        // ¿La apuesta gana con este número?
        private static bool EsGanadora(clsApuestaRuleta a, int numero)
        {
            switch (a.TipoApuesta)
            {
                case "Numero":
                    return int.Parse(a.ValorApuesta) == numero;

                case "Color":
                    return numero != 0 && ObtenerColor(numero) == a.ValorApuesta;

                case "ParImpar":
                    if (numero == 0) return false;
                    return a.ValorApuesta == (numero % 2 == 0 ? "Par" : "Impar");

                case "Docena":
                    int docena = int.Parse(a.ValorApuesta);   // 1, 2 o 3
                    return numero >= (docena - 1) * 12 + 1 && numero <= docena * 12;

                default:
                    return false;
            }
        }

        // Cuánto paga cada tipo de apuesta (por cada ficha apostada)
        private static int ObtenerPago(string tipo) => tipo switch
        {
            "Numero" => 35,
            "Docena" => 2,
            _ => 1          // Color y ParImpar
        };

        private static string ObtenerColor(int numero)
        {
            if (numero == 0) return "Verde";
            return NumerosRojos.Contains(numero) ? "Rojo" : "Negro";
        }
    }
}