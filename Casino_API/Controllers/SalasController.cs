using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Salas de Blackjack: crear, listar, unirse y salir (RB1, RNF4)
    [ApiController]
    [Route("api/[controller]")]   // api/salas
    [Authorize]
    public class SalasController : ControllerBase
    {
        private readonly ISalaLN _salaLN;

        public SalasController(ISalaLN salaLN)
        {
            _salaLN = salaLN;
        }

        private int IdUsuario => int.Parse(User.FindFirst("sub")!.Value);

        // GET api/salas → salas públicas activas
        [HttpGet]
        public async Task<IActionResult> ListarPublicas()
        {
            var respuesta = await _salaLN.ListarPublicasAsync();
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // GET api/salas/mia → la sala donde estoy (para reconectar)
        [HttpGet("mia")]
        public async Task<IActionResult> MiSala()
        {
            var respuesta = await _salaLN.ObtenerMiSalaAsync(IdUsuario);
            return respuesta.Success ? Ok(respuesta) : NotFound(respuesta);
        }

        // GET api/salas/5 → detalle de una sala
        [HttpGet("{idSala:int}")]
        public async Task<IActionResult> Obtener(int idSala)
        {
            var respuesta = await _salaLN.ObtenerSalaAsync(idSala);
            return respuesta.Success ? Ok(respuesta) : NotFound(respuesta);
        }

        // POST api/salas → crear sala (el creador entra automáticamente)
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] clsCrearSalaRequest datos)
        {
            var respuesta = await _salaLN.CrearAsync(IdUsuario, datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // POST api/salas/unirse → unirse con el código
        [HttpPost("unirse")]
        public async Task<IActionResult> Unirse([FromBody] clsUnirseSalaRequest datos)
        {
            var respuesta = await _salaLN.UnirseAsync(IdUsuario, datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // POST api/salas/5/salir → salir (si queda vacía, se cierra)
        [HttpPost("{idSala:int}/salir")]
        public async Task<IActionResult> Salir(int idSala)
        {
            var respuesta = await _salaLN.SalirAsync(IdUsuario, idSala);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }
    }
}