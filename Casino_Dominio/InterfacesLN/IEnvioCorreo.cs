namespace Casino_Dominio.InterfacesLN
{
    // La lógica usa esta interfaz para mandar correos.
    // La implementa la API (EmailService con SMTP), así LogicaNegocios no depende del servidor de correo.
    public interface IEnvioCorreo
    {
        Task<bool> EnviarCodigoVerificacionAsync(string correo, string nombre, string codigo, int minutosValidez);
    }
}
