using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Casino_API.Hubs
{
    // Punto de conexión en tiempo real del Blackjack 
    // La app se conecta a /hubs/blackjack con su token y llama estos métodos.
    [Authorize]
    public class BlackjackHub : Hub
    {
       
        private readonly IBlackjackLN _blackjackLN;

        // Cuántas conexiones abiertas tiene cada usuario (IdUsuario → cantidad)
        private static readonly ConcurrentDictionary<int, int> _conexiones = new();

        public BlackjackHub(IBlackjackLN blackjackLN)
        {
            _blackjackLN = blackjackLN;
        }

        // Id del usuario que viene en el token
        private int IdUsuario => int.Parse(Context.User!.FindFirst("sub")!.Value);

        // Nombre del grupo de cada sala: todos los de la sala 5 están en "sala-5"
        public static string Grupo(int idSala) => $"sala-{idSala}";

        // Métodos que la app puede llamar

        // Sentarse en la mesa (ya debe haberse unido a la sala por la API normal)
        public async Task<Respuesta<clsEstadoMesa>> EntrarMesa(int idSala)
        {
            // Primero se agrega al grupo para que reciba los avisos de la sala
            await Groups.AddToGroupAsync(Context.ConnectionId, Grupo(idSala));

            var respuesta = await _blackjackLN.EntrarAsync(idSala, IdUsuario);

            if (!respuesta.Success)
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(idSala));

            return respuesta;
        }

        public async Task<Respuesta<bool>> SalirMesa(int idSala)
        {
            var respuesta = await _blackjackLN.SalirAsync(idSala, IdUsuario);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Grupo(idSala));
            return respuesta;
        }

        public Task<Respuesta<bool>> Apostar(int idSala, decimal monto)
            => _blackjackLN.ApostarAsync(idSala, IdUsuario, monto);

        public Task<Respuesta<bool>> Pedir(int idSala)
            => _blackjackLN.AccionAsync(idSala, IdUsuario, "Pedir");

        public Task<Respuesta<bool>> Plantarse(int idSala)
            => _blackjackLN.AccionAsync(idSala, IdUsuario, "Plantarse");

        public Task<Respuesta<bool>> Doblar(int idSala)
            => _blackjackLN.AccionAsync(idSala, IdUsuario, "Doblar");

        public Task<Respuesta<bool>> Dividir(int idSala)
            => _blackjackLN.AccionAsync(idSala, IdUsuario, "Dividir");
      
        
        // Se abrió una conexión: suma 1 al contador del usuario
        public override async Task OnConnectedAsync()
        {
            _conexiones.AddOrUpdate(IdUsuario, 1, (_, cantidad) => cantidad + 1);
            await base.OnConnectedAsync();
        }

        // Se cayó una conexión. Solo se marca desconectado si era la ÚLTIMA (RNF6)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int quedan = _conexiones.AddOrUpdate(IdUsuario, 0, (_, cantidad) => Math.Max(0, cantidad - 1));

            if (quedan == 0)
            {
                _conexiones.TryRemove(new KeyValuePair<int, int>(IdUsuario, 0));
                await _blackjackLN.DesconectarAsync(IdUsuario);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}