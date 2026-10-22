using Casino_Dominio.Clases;
using Casino_Dominio.Clases.Historial;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;

namespace Casino_LogicaNegocios.Implementaciones
{
    // Historial de compras y jugadas del usuario (RG5)
    public class HistorialLN : IHistorialLN
    {
        private readonly IPaProcedimientoAD _procedimientos;
        private readonly ILogger<HistorialLN> _logger;

        public HistorialLN(IPaProcedimientoAD procedimientos, ILogger<HistorialLN> logger)
        {
            _procedimientos = procedimientos;
            _logger = logger;
        }

        public async Task<Respuesta<IEnumerable<clsHistorialCompra>>> ComprasAsync(int idUsuario, int cantidad)
        {
            var respuesta = await _procedimientos.HistorialComprasAsync(idUsuario, LimitarCantidad(cantidad));

            if (!respuesta.Success)
                _logger.LogError("Error en historial de compras del usuario {IdUsuario}: {Error}", idUsuario, respuesta.Error);

            return respuesta;
        }

        public async Task<Respuesta<IEnumerable<clsHistorialJuego>>> JuegosAsync(int idUsuario, int cantidad)
        {
            var respuesta = await _procedimientos.HistorialJuegosAsync(idUsuario, LimitarCantidad(cantidad));

            if (!respuesta.Success)
                _logger.LogError("Error en historial de juegos del usuario {IdUsuario}: {Error}", idUsuario, respuesta.Error);

            return respuesta;
        }

        // Entre 1 y 100 registros
        private static int LimitarCantidad(int cantidad) => Math.Clamp(cantidad, 1, 100);
    }
}