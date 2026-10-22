using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Ruleta individual (RR1, RR2, RR3)
    [ApiController]
    [Route("api/[controller]")]   // api/ruleta
    [Authorize]
    public class RuletaController : ControllerBase
    {
        private readonly IRuletaLN _ruletaLN;

        public RuletaController(IRuletaLN ruletaLN)
        {
            _ruletaLN = ruletaLN;
        }

        // POST api/ruleta/girar → recibe las apuestas, gira y paga
        [HttpPost("girar")]
        public async Task<IActionResult> Girar([FromBody] clsGiroRequest datos)
        {
            int idUsuario = int.Parse(User.FindFirst("sub")!.Value);

            var respuesta = await _ruletaLN.GirarAsync(idUsuario, datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }
    }
}