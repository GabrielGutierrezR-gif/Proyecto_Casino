using Microsoft.AspNetCore.SignalR;

namespace Casino_API.Servicios
{
    // Le dice a SignalR que el "usuario" de cada conexión es el "sub" del token (el IdUsuario).
    // Así Clients.User("5") le llega solo al usuario 5, aunque tenga la app abierta en 2 teléfonos.
    public class UsuarioIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
            => connection.User?.FindFirst("sub")?.Value;
    }
}