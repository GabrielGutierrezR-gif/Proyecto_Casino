using Casino_API.Hubs;
using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.SignalR;

namespace Casino_API.Servicios
{
    // Implementa INotificadorBlackjack usando SignalR.
    // BlackjackLN dice "avisa a la sala 5" y esta clase lo hace llegar a los teléfonos.
    public class NotificadorSignalR : INotificadorBlackjack
    {
        private readonly IHubContext<BlackjackHub> _hub;

        public NotificadorSignalR(IHubContext<BlackjackHub> hub)
        {
            _hub = hub;
        }

        // A todos los de la sala: cómo está la mesa ahora
        public Task EnviarEstadoAsync(int idSala, clsEstadoMesa estado)
            => _hub.Clients.Group(BlackjackHub.Grupo(idSala)).SendAsync("EstadoMesa", estado);

        // A todos los de la sala: resultados de la ronda
        public Task EnviarRondaFinalizadaAsync(int idSala, clsResultadoRonda resultado)
            => _hub.Clients.Group(BlackjackHub.Grupo(idSala)).SendAsync("RondaFinalizada", resultado);

        // A todos los de la sala: mensajes tipo "Gabriel apostó 50 fichas"
        public Task EnviarMensajeAsync(int idSala, string mensaje)
            => _hub.Clients.Group(BlackjackHub.Grupo(idSala)).SendAsync("Mensaje", mensaje);

        // Solo a ese usuario: su saldo actualizado
        public Task EnviarSaldoAsync(int idUsuario, decimal saldo)
            => _hub.Clients.User(idUsuario.ToString()).SendAsync("SaldoActualizado", saldo);
    }
}