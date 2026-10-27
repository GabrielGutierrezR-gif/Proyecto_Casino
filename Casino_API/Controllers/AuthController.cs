using Casino_API.Hubs;
using Casino_API.Servicios;
using Casino_Dominio.Clases;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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
        private readonly IHubContext<BlackjackHub> _hub;

        // La API inyecta la lógica de usuarios, el generador de tokens y el Hub (registrados en Program.cs)
        public AuthController(IUsuarioLN usuarioLN, TokenService tokenService, IHubContext<BlackjackHub> hub)
        {
            _usuarioLN = usuarioLN;
            _tokenService = tokenService;
            _hub = hub;
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
        // Paso 2: con el código del correo se crea la cuenta, con las fichas de bienvenida
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] clsRegistro datos)
        {
            var respuesta = await _usuarioLN.RegistrarAsync(datos);

            // 400: datos inválidos, código incorrecto o correo ya registrado
            if (!respuesta.Success || respuesta.Data == null)
                return BadRequest(respuesta);

            // 200: usuario creado, ya queda con sesión iniciada
            return await CrearSesionAsync(respuesta.Data);
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

            // La primera vez la app muestra la bienvenida y el regalo de fichas
            bool primerIngreso = await _usuarioLN.MarcarBienvenidaAsync(respuesta.Data.IdUsuario);
            return await CrearSesionAsync(respuesta.Data, primerIngreso);
        }

        // Crea una sesión NUEVA (la del otro dispositivo deja de valer) y arma la respuesta con el token.
        // La app guarda el token y lo envía en las siguientes peticiones (RNF6).
        private async Task<IActionResult> CrearSesionAsync(clsPerfilUsuario usuario, bool primerIngreso = false)
        {
            // 1) Código de sesión nuevo guardado en la BD
            var sesion = await _usuarioLN.IniciarSesionUnicaAsync(usuario.IdUsuario);

            if (!sesion.Success)
                return BadRequest(Respuesta<clsSesion>.Fail(sesion.Error));

            // 2) Si el otro teléfono estaba en una mesa de Blackjack, se le avisa al instante
            await _hub.Clients.User(usuario.IdUsuario.ToString())
                .SendAsync("SesionCerrada", "Se inició sesión con tu cuenta en otro dispositivo.");

            // 3) Token con el código de sesión adentro
            return Ok(Respuesta<clsSesion>.Ok(new clsSesion
            {
                Token = _tokenService.GenerarToken(usuario, sesion.Data),
                Usuario = usuario,
                PrimerIngreso = primerIngreso,
                // En el primer ingreso el saldo es justo el regalo de bienvenida
                FichasBienvenida = primerIngreso ? (int)usuario.SaldoFichas : 0
            }));
        }
    }
}