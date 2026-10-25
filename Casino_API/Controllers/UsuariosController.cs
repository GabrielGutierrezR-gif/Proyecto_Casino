
using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
	// Controlador de usuarios: datos del jugador que ya inició sesión.
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class UsuariosController : ControllerBase
	{
		private readonly IUsuarioLN _usuarioLN;

		public UsuariosController(IUsuarioLN usuarioLN)
		{
			_usuarioLN = usuarioLN;
		}

		// GET api/usuarios/perfil
		// Obtiene el perfil del usuario autenticado.
		[HttpGet("perfil")]
		public async Task<IActionResult> Perfil()
		{
			var claim = User.FindFirst("sub")?.Value;

			if (!int.TryParse(claim, out int idUsuario))
				return Unauthorized();

			var respuesta = await _usuarioLN.ObtenerPerfilAsync(idUsuario);

			if (!respuesta.Success)
				return NotFound(respuesta);

			return Ok(respuesta);
		}

		// PUT api/usuarios/nombre
		// Actualiza el nombre del usuario autenticado.
		[HttpPut("nombre")]
		public async Task<IActionResult> ActualizarNombre(
			[FromBody] ActualizarNombreRequest datos)
		{
			if (datos == null ||
				string.IsNullOrWhiteSpace(datos.NuevoNombre))
			{
				return BadRequest(new
				{
					success = false,
					error = "Debe ingresar un nombre de usuario."
				});
			}

			var claim = User.FindFirst("sub")?.Value;

			if (!int.TryParse(claim, out int idUsuario))
				return Unauthorized();

			var respuesta =
				await _usuarioLN.ActualizarNombreUsuarioAsync(
					idUsuario,
					datos.NuevoNombre);

			if (!respuesta.Success)
				return BadRequest(respuesta);

			return Ok(respuesta);
		}

		// Datos que recibe el endpoint para actualizar el nombre.
		public class ActualizarNombreRequest
		{
			public string NuevoNombre { get; set; } = string.Empty;
		}
	}
}
