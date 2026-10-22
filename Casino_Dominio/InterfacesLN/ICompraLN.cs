using Casino_Dominio.Clases;
using Casino_Dominio.EntidadesTipadas;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesLN
{
    public interface ICompraLN
    {
        Task<Respuesta<IEnumerable<TPaquetesFichas>>> ListarPaquetesAsync();

        Task<Respuesta<clsMetodoPago>> AgregarMetodoPagoAsync(int idUsuario, clsMetodoPagoRequest datos);

        Task<Respuesta<IEnumerable<clsMetodoPago>>> ListarMetodosPagoAsync(int idUsuario);

        Task<Respuesta<clsCompraResultado>> ComprarPaqueteAsync(int idUsuario, clsCompraRequest datos);
    }
}