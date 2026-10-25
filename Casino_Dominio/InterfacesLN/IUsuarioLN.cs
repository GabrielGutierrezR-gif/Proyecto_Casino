using Casino_Dominio.Clases;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesLN
{
    public interface IUsuarioLN
    {
        // Paso 1 del registro: valida los datos y manda el código al correo
        Task<Respuesta<bool>> EnviarCodigoRegistroAsync(clsRegistro datos);
        // Paso 2: con el código correcto crea la cuenta
        Task<Respuesta<clsPerfilUsuario>> RegistrarAsync(clsRegistro datos);
        // true si es la primera vez que entra (y la marca como vista)
        Task<bool> MarcarBienvenidaAsync(int idUsuario);

        Task<Respuesta<clsPerfilUsuario>> LoginAsync(clsLogin datos);

        Task<Respuesta<clsPerfilUsuario>> ObtenerPerfilAsync(int idUsuario);

		Task<Respuesta<clsPerfilUsuario>> ActualizarNombreUsuarioAsync(
	   int idUsuario,
	   string nuevoNombre);
	}
}