using AutoMapper;
using Casino_Dominio.Clases;
using Casino_Dominio.Entidades;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_Utilitarios;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace Casino_LogicaNegocios.Implementaciones
{
    public class UsuarioLN : IUsuarioLN
    {
        private readonly IUnidadTrabajoEF _unidadDeTrabajo;
        private readonly ILogger<UsuarioLN> _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguracionLN _configuracion;
        private readonly IVerificacionCorreoLN _verificacion;
        private readonly IEnvioCorreo _envioCorreo;

        public UsuarioLN(
            IUnidadTrabajoEF unidadTrabajo,
            ILogger<UsuarioLN> logger,
            IMapper mapper,
            IConfiguracionLN configuracion,
            IVerificacionCorreoLN verificacion,
            IEnvioCorreo envioCorreo)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
            _mapper = mapper;
            _configuracion = configuracion;
            _verificacion = verificacion;
            _envioCorreo = envioCorreo;
        }

        // ========== Registro, paso 1: enviar el código al correo ==========
        public async Task<Respuesta<bool>> EnviarCodigoRegistroAsync(clsRegistro datos)
        {
            var resultado = new Respuesta<bool>();

            try
            {
                // Se valida todo antes de mandar el correo, así el usuario corrige sus datos primero
                string? error = await ValidarDatosRegistroAsync(datos);
                if (error != null)
                {
                    resultado.Success = false;
                    resultado.Error = error;
                    return resultado;
                }

                var correo = datos.Correo.Trim().ToLower();
                var (codigo, errorCodigo) = _verificacion.Generar(correo);

                if (codigo == null)
                {
                    resultado.Success = false;
                    resultado.Error = errorCodigo;
                    return resultado;
                }

                bool enviado = await _envioCorreo.EnviarCodigoVerificacionAsync(
                    correo, datos.NombreUsuario.Trim(), codigo, _verificacion.MinutosValidez);

                if (!enviado)
                {
                    resultado.Success = false;
                    resultado.Error = "No se pudo enviar el correo. Revisa la dirección e inténtalo de nuevo.";
                    return resultado;
                }

                resultado.Data = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el código de registro a {Correo}", datos.Correo);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Registro ==========
        public async Task<Respuesta<clsPerfilUsuario>> RegistrarAsync(clsRegistro datos)
        {
            var resultado = new Respuesta<clsPerfilUsuario>();

            try
            {
                // 1) Validaciones (las mismas del paso 1)
                string? error = await ValidarDatosRegistroAsync(datos);
                if (error != null)
                {
                    resultado.Success = false;
                    resultado.Error = error;
                    return resultado;
                }

                var nombre = datos.NombreUsuario.Trim();
                var correo = datos.Correo.Trim().ToLower();

                // 2) El código que llegó al correo debe ser correcto
                var (codigoValido, errorCodigo) = _verificacion.Verificar(correo, datos.Codigo);
                if (!codigoValido)
                {
                    resultado.Success = false;
                    resultado.Error = errorCodigo;
                    return resultado;
                }

                // 3) Fichas de bienvenida desde la tabla Configuracion 
                int fichasBienvenida = await _configuracion.ObtenerEnteroAsync("FichasBienvenida", 500);

                // 4) Crear el usuario con la contraseña encriptada (RNF3)
                var usuario = new Usuario
                {
                    NombreUsuario = nombre,
                    Correo = correo,
                    Password = BCrypt.Net.BCrypt.HashPassword(datos.Password),
                    SaldoFichas = fichasBienvenida,
                    FechaRegistro = DateTime.Now,
                    Rol = "Jugador"
                };

                var respuestaRepositorio = await _unidadDeTrabajo.TUsuario.InsertarAsync(usuario);

                if (!respuestaRepositorio.Success || respuestaRepositorio.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = respuestaRepositorio.Error;
                    return resultado;
                }

                resultado.Data = _mapper.Map<clsPerfilUsuario>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario {Correo}", datos.Correo);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Primer ingreso: mostrar la bienvenida una sola vez ==========
        public async Task<bool> MarcarBienvenidaAsync(int idUsuario)
        {
            try
            {
                var usuario = await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);

                if (!usuario.Success || usuario.Data == null || usuario.Data.BienvenidaVista)
                    return false;

                usuario.Data.BienvenidaVista = true;
                var modificar = await _unidadDeTrabajo.TUsuario.ModificarAsync(usuario.Data);
                return modificar.Success;
            }
            catch (Exception ex)
            {
                // Si falla, no se impide el login: solo no se muestra la bienvenida
                _logger.LogError(ex, "Error al marcar la bienvenida del usuario {IdUsuario}", idUsuario);
                return false;
            }
        }

        // Devuelve el error o null si los datos del registro están bien
        private async Task<string?> ValidarDatosRegistroAsync(clsRegistro datos)
        {
            var nombre = datos.NombreUsuario?.Trim() ?? "";
            var correo = datos.Correo?.Trim().ToLower() ?? "";

            if (nombre.Length < 3 || nombre.Length > 50)
                return "El nombre de usuario debe tener entre 3 y 50 caracteres.";

            if (!CorreoValido(correo))
                return "El correo no tiene un formato válido.";

            if (string.IsNullOrWhiteSpace(datos.Password) || datos.Password.Length < 6)
                return "La contraseña debe tener al menos 6 caracteres.";

            var existe = await _unidadDeTrabajo.TUsuario.ContarAsync(x => x.Correo == correo);

            if (!existe.Success)
                return existe.Error;

            if (existe.Data > 0)
                return "El correo ya está registrado.";

            return null;
        }

        // ========== RG1: Login ==========
        public async Task<Respuesta<clsPerfilUsuario>> LoginAsync(clsLogin datos)
        {
            var resultado = new Respuesta<clsPerfilUsuario>();

            try
            {
                var correo = datos.Correo?.Trim().ToLower() ?? "";

                if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(datos.Password))
                {
                    resultado.Success = false;
                    resultado.Error = "Debe ingresar el correo y la contraseña.";
                    return resultado;
                }

                var respuestaRepositorio =
                    await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(x => x.Correo == correo);

                // Mismo mensaje si no existe el correo o si la contraseña está mal:
                // así nadie puede averiguar qué correos están registrados.
                if (!respuestaRepositorio.Success ||
                    respuestaRepositorio.Data == null ||
                    !BCrypt.Net.BCrypt.Verify(datos.Password, respuestaRepositorio.Data.Password))
                {
                    resultado.Success = false;
                    resultado.Error = "Correo o contraseña incorrectos.";
                    return resultado;
                }

                resultado.Data = _mapper.Map<clsPerfilUsuario>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login {Correo}", datos.Correo);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

        // ========== Una sola sesión por cuenta ==========
        public async Task<Respuesta<Guid>> IniciarSesionUnicaAsync(int idUsuario)
        {
            try
            {
                var usuario = await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);

                if (!usuario.Success || usuario.Data == null)
                    return Respuesta<Guid>.Fail("Usuario no encontrado.");

                // Código nuevo: el token del otro teléfono queda con el código viejo
                var sesion = Guid.NewGuid();
                usuario.Data.SesionActual = sesion;

                var modificar = await _unidadDeTrabajo.TUsuario.ModificarAsync(usuario.Data);
                if (!modificar.Success)
                    return Respuesta<Guid>.Fail(modificar.Error);

                return Respuesta<Guid>.Ok(sesion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión del usuario {IdUsuario}", idUsuario);
                return Respuesta<Guid>.Fail(ex.GetBaseException().Message);
            }
        }



        // ========== RG2: Perfil y saldo ==========
        public async Task<Respuesta<clsPerfilUsuario>> ObtenerPerfilAsync(int idUsuario)
        {
            var resultado = new Respuesta<clsPerfilUsuario>();

            try
            {
                var respuestaRepositorio =
                    await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(x => x.IdUsuario == idUsuario);

                if (!respuestaRepositorio.Success || respuestaRepositorio.Data == null)
                {
                    resultado.Success = false;
                    resultado.Error = "Usuario no encontrado.";
                    return resultado;
                }

                resultado.Data = _mapper.Map<clsPerfilUsuario>(respuestaRepositorio.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil {IdUsuario}", idUsuario);
                resultado.Success = false;
                resultado.Error = ex.GetBaseException().Message;
            }

            return resultado;
        }

		// Actualizar nombre de usuario
		public async Task<Respuesta<clsPerfilUsuario>> ActualizarNombreUsuarioAsync(
			int idUsuario,
			string nuevoNombre)
		{
			var resultado = new Respuesta<clsPerfilUsuario>();

			try
			{
				var nombre = nuevoNombre?.Trim() ?? "";

				if (nombre.Length < 3 || nombre.Length > 50)
				{
					resultado.Success = false;
					resultado.Error =
						"El nombre de usuario debe tener entre 3 y 50 caracteres.";
					return resultado;
				}

				// Buscar al usuario que inició sesión
				var respuestaUsuario =
					await _unidadDeTrabajo.TUsuario.ObtenerEntidadAsync(
						x => x.IdUsuario == idUsuario);

				if (!respuestaUsuario.Success || respuestaUsuario.Data == null)
				{
					resultado.Success = false;
					resultado.Error = "Usuario no encontrado.";
					return resultado;
				}

				// Comprobar si otro usuario ya utiliza ese nombre
				var respuestaExistente =
					await _unidadDeTrabajo.TUsuario.ContarAsync(
						x => x.NombreUsuario == nombre && x.IdUsuario != idUsuario);

				if (!respuestaExistente.Success)
				{
					resultado.Success = false;
					resultado.Error = respuestaExistente.Error;
					return resultado;
				}

				if (respuestaExistente.Data.GetValueOrDefault() > 0)
				{
					resultado.Success = false;
					resultado.Error = "Ese nombre de usuario ya está en uso.";
					return resultado;
				}

				// Cambiar y guardar el nombre
				var usuario = respuestaUsuario.Data;
				usuario.NombreUsuario = nombre;

				var respuestaModificacion =
					await _unidadDeTrabajo.TUsuario.ModificarAsync(usuario);

				if (!respuestaModificacion.Success || respuestaModificacion.Data == null)
				{
					resultado.Success = false;
					resultado.Error = respuestaModificacion.Error
						?? "No se pudo actualizar el nombre de usuario.";
					return resultado;
				}

				resultado.Data =
					_mapper.Map<clsPerfilUsuario>(respuestaModificacion.Data);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					"Error al actualizar nombre de usuario {IdUsuario}", idUsuario);

				resultado.Success = false;
				resultado.Error = ex.GetBaseException().Message;
			}

			return resultado;
		}

		// ========== Métodos de apoyo ==========


		private static bool CorreoValido(string correo)
        {
            try
            {
                var direccion = new MailAddress(correo);
                return direccion.Address == correo;
            }
            catch
            {
                return false;
            }
        }
    }
}