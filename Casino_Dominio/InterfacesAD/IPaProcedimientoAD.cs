using Casino_Dominio.Clases;
using Casino_Dominio.Clases.Historial;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesAD
{
    // Acceso a los procedimientos almacenados
    public interface IPaProcedimientoAD
    {
        Task<Respuesta<IEnumerable<clsHistorialCompra>>> HistorialComprasAsync(int idUsuario, int cantidad);

        Task<Respuesta<IEnumerable<clsHistorialJuego>>> HistorialJuegosAsync(int idUsuario, int cantidad);
    }
}