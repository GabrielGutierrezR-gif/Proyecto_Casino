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

        // POST api/auth/enviar-codigo
        // Paso 1 del registro: valida los datos y manda un código de 6 dígitos al correo
        [HttpPost("enviar-codigo")]
        public async Task<IActionResult> EnviarCodigo([FromBody] clsRegistro datos)
        {
            var respuesta = await _usuarioLN.EnviarCodigoRegistroAsync(datos);
            return respuesta.Success ? Ok(respuesta) : BadRequest(respuesta);
        }

        // POST api/auth/registro   (RG1 + RG2)
        // Paso 2: con el código del correo se crea la cuenta
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

            // 200: login correcto. La primera vez la app muestra la bienvenida y el regalo de fichas
            bool primerIngreso = await _usuarioLN.MarcarBienvenidaAsync(respuesta.Data.IdUsuario);
            return Ok(CrearSesion(respuesta.Data, primerIngreso));
        }

        // Arma la respuesta con el token JWT + los datos del perfil.
        // La app guarda el token y lo envía en las siguientes peticiones (RNF6).
        private Respuesta<clsSesion> CrearSesion(clsPerfilUsuario usuario, bool primerIngreso = false)
        {
            return Respuesta<clsSesion>.Ok(new clsSesion
            {
                Token = _tokenService.GenerarToken(usuario),
                Usuario = usuario,
                PrimerIngreso = primerIngreso,
                // En el primer ingreso el saldo es justo el regalo de bienvenida
                FichasBienvenida = primerIngreso ? (int)usuario.SaldoFichas : 0
            });
        }
    }
}