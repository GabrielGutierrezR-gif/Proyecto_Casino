using Casino_Dominio.Clases;
using Casino_Utilitarios;

namespace Casino_Dominio.InterfacesLN
{
    public interface IBlackjackLN
    {
        // Entrar a la mesa (ya debe ser miembro de la sala) y recibir el estado actual
        Task<Respuesta<clsEstadoMesa>> EntrarAsync(int idSala, int idUsuario);

        // Salir de la sala (si está jugando, su mano se planta y sale al terminar la ronda)
        Task<Respuesta<bool>> SalirAsync(int idSala, int idUsuario);

        // Se cayó la conexión (puede volver a entrar, RNF6)
        Task DesconectarAsync(int idUsuario);

        // Apuesta inicial de la ronda (RB3)
        Task<Respuesta<bool>> ApostarAsync(int idSala, int idUsuario, decimal monto);

        // Iniciar la ronda sin esperar a los que no apostaron
        Task<Respuesta<bool>> IniciarRondaAsync(int idSala, int idUsuario);

        // Pedir, Plantarse, Doblar, Dividir (RB3)
        Task<Respuesta<bool>> AccionAsync(int idSala, int idUsuario, string accion);

        // Al arrancar la API: devolver fichas de rondas que quedaron a medias
        Task CancelarRondasPendientesAsync();
    }
}