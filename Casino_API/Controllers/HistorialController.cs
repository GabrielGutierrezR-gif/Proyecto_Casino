using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Historial del usuario (RG5)
    [ApiController]
    [Route("api/[controller]")]   // api/historial
    [Authorize]
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialLN _historialLN;

        public HistorialController(IHistorialLN historialLN)
        {
            _historialLN = historialLN;
        }

        // El usuario sale del token: cada quien solo ve SU historial
        private int IdUsuario => int.Parse(User.FindFirst("sub")!.Value);

        // GET api/historial/compras?cantidad=20
        [HttpGet("compras")]
        public async Task<IActionResult> Compras([FromQuery] int cantidad = 20)
        {
            var respuesta = await _historialLN.ComprasAsync(IdUsuario, cantidad);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // GET api/historial/juegos?cantidad=20   (ruleta + blackjack)
        [HttpGet("juegos")]
        public async Task<IActionResult> Juegos([FromQuery] int cantidad = 20)
        {
            var respuesta = await _historialLN.JuegosAsync(IdUsuario, cantidad);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }
    }
}