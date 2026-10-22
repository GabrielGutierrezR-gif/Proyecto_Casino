using Casino_Dominio.InterfacesLN;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Controlador de usuarios: datos del jugador que ya inició sesión.
    [ApiController]
    [Route("api/[controller]")]   // la ruta queda: api/usuarios
    [Authorize]                   // todo este controlador exige token; sin token responde 401
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioLN _usuarioLN;

        // La API inyecta la lógica de usuarios (registrada en Program.cs)
        public UsuariosController(IUsuarioLN usuarioLN)
        {
            _usuarioLN = usuarioLN;
        }

        // GET api/usuarios/perfil   (RG2)
        // Devuelve nombre, correo, rol y saldo de fichas del usuario
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            // El Id sale del token ("sub"), no de la URL.
            // Así cada usuario solo puede ver su propio perfil.
            int idUsuario = int.Parse(User.FindFirst("sub")!.Value);

            var respuesta = await _usuarioLN.ObtenerPerfilAsync(idUsuario);

            // 404: el usuario del token ya no existe
            if (!respuesta.Success)
                return NotFound(respuesta);

            // 200: perfil con el saldo actualizado
            return Ok(respuesta);
        }
    }
}