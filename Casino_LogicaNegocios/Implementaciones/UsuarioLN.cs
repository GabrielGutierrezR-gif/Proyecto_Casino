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

        public UsuarioLN(
            IUnidadTrabajoEF unidadTrabajo,
            ILogger<UsuarioLN> logger,
            IMapper mapper,
            IConfiguracionLN configuracion)
        {
            _unidadDeTrabajo = unidadTrabajo;
            _logger = logger;
            _mapper = mapper;
            _configuracion = configuracion;
        }

        // ========== Registro ==========
        public async Task<Respuesta<clsPerfilUsuario>> RegistrarAsync(clsRegistro datos)
        {
            var resultado = new Respuesta<clsPerfilUsuario>();

            try
            {
                // 1) Validaciones
                var nombre = datos.NombreUsuario?.Trim() ?? "";
                var correo = datos.Correo?.Trim().ToLower() ?? "";

                if (nombre.Length < 3 || nombre.Length > 50)
                {
                    resultado.Success = false;
                    resultado.Error = "El nombre de usuario debe tener entre 3 y 50 caracteres.";
                    return resultado;
                }

                if (!CorreoValido(correo))
                {
                    resultado.Success = false;
                    resultado.Error = "El correo no tiene un formato válido.";
                    return resultado;
                }

                if (string.IsNullOrWhiteSpace(datos.Password) || datos.Password.Length < 6)
                {
                    resultado.Success = false;
                    resultado.Error = "La contraseña debe tener al menos 6 caracteres.";
                    return resultado;
                }

                // 2) ¿El correo ya existe?
                var existe = await _unidadDeTrabajo.TUsuario.ContarAsync(x => x.Correo == correo);

                if (!existe.Success)
                {
                    resultado.Success = false;
                    resultado.Error = existe.Error;
                    return resultado;
                }

                if (existe.Data > 0)
                {
                    resultado.Success = false;
                    resultado.Error = "El correo ya está registrado.";
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