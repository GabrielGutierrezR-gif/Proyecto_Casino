using Casino_Dominio.Clases;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesLN
{
    public interface IUsuarioLN
    {
        Task<Respuesta<clsPerfilUsuario>> RegistrarAsync(clsRegistro datos);

        Task<Respuesta<clsPerfilUsuario>> LoginAsync(clsLogin datos);

        Task<Respuesta<clsPerfilUsuario>> ObtenerPerfilAsync(int idUsuario);
    }
}