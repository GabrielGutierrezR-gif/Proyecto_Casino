using Casino_API.Servicios;
using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.AspNetCore.Mvc;

namespace Casino_API.Controllers
{
    // Controlador de autenticación: registro e inicio de sesión.
    // No lleva [Authorize] porque el usuario todavía no tiene token.
    [ApiController]
    [Route("api/[controller]")]   // la ruta queda: api/auth
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioLN _usuarioLN;
        private readonly TokenService _tokenService;

        // La API inyecta la lógica de usuarios y el generador de tokens (registrados en Program.cs)
        public AuthController(IUsuarioLN usuarioLN, TokenService tokenService)
        {
            _usuarioLN = usuarioLN;
            _tokenService = tokenService;
        }

        // POST api/auth/registro   (RG1 + RG2)
        // Crea la cuenta, le da las fichas de bienvenida y devuelve el token
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] clsRegistro datos)
        {
            var respuesta = await _usuarioLN.RegistrarAsync(datos);

            // 400: datos inválidos o correo ya registrado
            if (!respuesta.Success || respuesta.Data == null)
                return BadRequest(respuesta);

            // 200: usuario creado, ya queda con sesión iniciada
            return Ok(CrearSesion(respuesta.Data));
        }

        // POST api/auth/login   (RG1)
        // Valida correo y contraseña y devuelve el token
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] clsLogin datos)
        {
            var respuesta = await _usuarioLN.LoginAsync(datos);

            // 401: correo o contraseña incorrectos
            if (!respuesta.Success || respuesta.Data == null)
                return Unauthorized(respuesta);

            // 200: login correcto
            return Ok(CrearSesion(respuesta.Data));
        }

        // Arma la respuesta con el token JWT + los datos del perfil.
        // La app guarda el token y lo envía en las siguientes peticiones (RNF6).
        private Respuesta<clsSesion> CrearSesion(clsPerfilUsuario usuario)
        {
            return Respuesta<clsSesion>.Ok(new clsSesion
            {
                Token = _tokenService.GenerarToken(usuario),
                Usuario = usuario
            });
        }
    }
}