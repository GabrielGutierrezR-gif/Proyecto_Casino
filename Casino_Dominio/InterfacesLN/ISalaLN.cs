using Casino_Dominio.Clases;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesLN
{
    public interface ISalaLN
    {
        Task<Respuesta<IEnumerable<clsSala>>> ListarPublicasAsync();

        Task<Respuesta<clsSala>> ObtenerSalaAsync(int idSala);

        Task<Respuesta<clsSala>> ObtenerMiSalaAsync(int idUsuario);

        Task<Respuesta<clsSala>> CrearAsync(int idUsuario, clsCrearSalaRequest datos);

        Task<Respuesta<clsSala>> UnirseAsync(int idUsuario, clsUnirseSalaRequest datos);

        Task<Respuesta<bool>> SalirAsync(int idUsuario, int idSala);
    }
}