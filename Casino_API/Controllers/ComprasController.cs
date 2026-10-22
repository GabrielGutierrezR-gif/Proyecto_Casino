using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Compras de fichas y métodos de pago (RG3)
    [ApiController]
    [Route("api/[controller]")]   // api/compras
    [Authorize]                   // todo exige token
    public class ComprasController : ControllerBase
    {
        private readonly ICompraLN _compraLN;

        public ComprasController(ICompraLN compraLN)
        {
            _compraLN = compraLN;
        }

        // Id del usuario que viene en el token
        private int IdUsuario => int.Parse(User.FindFirst("sub")!.Value);

        // GET api/compras/paquetes → catálogo (Básico, Medio, Grande)
        [HttpGet("paquetes")]
        public async Task<IActionResult> Paquetes()
        {
            var respuesta = await _compraLN.ListarPaquetesAsync();
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // GET api/compras/metodos → mis tarjetas (enmascaradas)
        [HttpGet("metodos")]
        public async Task<IActionResult> MisMetodos()
        {
            var respuesta = await _compraLN.ListarMetodosPagoAsync(IdUsuario);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // POST api/compras/metodos → agregar tarjeta ficticia
        [HttpPost("metodos")]
        public async Task<IActionResult> AgregarMetodo([FromBody] clsMetodoPagoRequest datos)
        {
            var respuesta = await _compraLN.AgregarMetodoPagoAsync(IdUsuario, datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // POST api/compras → comprar un paquete
        [HttpPost]
        public async Task<IActionResult> Comprar([FromBody] clsCompraRequest datos)
        {
            var respuesta = await _compraLN.ComprarPaqueteAsync(IdUsuario, datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }
    }
}