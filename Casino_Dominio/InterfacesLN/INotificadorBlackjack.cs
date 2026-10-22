using Casino_Dominio.Clases;

namespace Casino_Dominio.InterfacesLN
{
    // La lógica usa esta interfaz para avisar a los jugadores en tiempo real 
    // La implementa la API con SignalR así LogicaNegocios no depende de SignalR.
    public interface INotificadorBlackjack
    {
        Task EnviarEstadoAsync(int idSala, clsEstadoMesa estado);
        Task EnviarRondaFinalizadaAsync(int idSala, clsResultadoRonda resultado);
        Task EnviarMensajeAsync(int idSala, string mensaje);
        Task EnviarSaldoAsync(int idUsuario, decimal saldo);
    }
}