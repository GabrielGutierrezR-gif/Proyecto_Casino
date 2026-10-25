using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Administración y configuración global del casino (REQ8)
    // Solo los usuarios con rol "Admin" pueden ver y cambiar los parámetros.
    [ApiController]
    [Route("api/[controller]")]   // api/admin
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguracionLN _configuracionLN;

        public AdminController(IConfiguracionLN configuracionLN)
        {
            _configuracionLN = configuracionLN;
        }

        // GET api/admin/configuracion → todos los parámetros (FichasBienvenida, MaxSalasActivas...)
        [HttpGet("configuracion")]
        public async Task<IActionResult> Listar()
        {
            var respuesta = await _configuracionLN.ListarAsync();
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // PUT api/admin/configuracion/FichasBienvenida   { "valor": "1000" }
        [HttpPut("configuracion/{clave}")]
        public async Task<IActionResult> Modificar(string clave, [FromBody] clsModificarConfiguracion datos)
        {
            var respuesta = await _configuracionLN.ModificarAsync(clave, datos.Valor?.Trim() ?? "");
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }
    }
}
