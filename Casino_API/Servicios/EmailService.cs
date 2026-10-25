using Casino_Dominio.InterfacesLN;
using System.Net;
using System.Net.Mail;

namespace Casino_API.Servicios
{
	// Envía los correos del casino por SMTP (sección "Correo" de appsettings).
	// En desarrollo, si no hay cuenta configurada, el código se escribe en la consola de la API.
	public class EmailService : IEnvioCorreo
	{
		private readonly IConfiguration _configuracion;
		private readonly IWebHostEnvironment _entorno;
		private readonly ILogger<EmailService> _logger;

		public EmailService(IConfiguration configuracion, IWebHostEnvironment entorno, ILogger<EmailService> logger)
		{
			_configuracion = configuracion;
			_entorno = entorno;
			_logger = logger;
		}

		public async Task<bool> EnviarCodigoVerificacionAsync(string correo, string nombre, string codigo, int minutosValidez)
		{
			var config = _configuracion.GetSection("Correo");
			string? usuario = config["Usuario"];
			string? password = config["Password"];

			if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
			{
				if (_entorno.IsDevelopment())
				{
					_logger.LogWarning(
						"Correo sin configurar. Código de verificación para {Correo}: {Codigo}", correo, codigo);
					return true;
				}

				_logger.LogError("Falta configurar la sección Correo (Usuario y Password) para enviar correos.");
				return false;
			}

			try
			{
				using var mensaje = new MailMessage
				{
					From = new MailAddress(
						string.IsNullOrWhiteSpace(config["Remitente"]) ? usuario : config["Remitente"]!,
						config["NombreRemitente"] ?? "Casino"),
					Subject = $"Tu código para crear la cuenta: {codigo}",
					Body = CrearCuerpo(nombre, codigo, minutosValidez),
					IsBodyHtml = true
				};
				mensaje.To.Add(correo);

				using var cliente = new SmtpClient(config["Servidor"] ?? "smtp.gmail.com", int.Parse(config["Puerto"] ?? "587"))
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(usuario, password)
				};

				await cliente.SendMailAsync(mensaje);
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "No se pudo enviar el código de verificación a {Correo}", correo);
				return false;
			}
		}

		private static string CrearCuerpo(string nombre, string codigo, int minutosValidez)
		{
			string nombreSeguro = WebUtility.HtmlEncode(nombre);

			return $"""
				<div style="font-family: Arial, sans-serif; max-width: 480px; margin: auto; padding: 24px;
				            background: #0d0d0d; color: #ffffff; border: 1px solid #69552e; border-radius: 14px;">
				  <h2 style="color: #f5c451; margin-top: 0;">CASINO</h2>
				  <p>Hola {nombreSeguro},</p>
				  <p>Usa este código para terminar de crear tu cuenta:</p>
				  <p style="font-size: 34px; font-weight: bold; letter-spacing: 8px; color: #f5c451; text-align: center;">
				    {codigo}
				  </p>
				  <p style="color: #aaaaaa;">El código vence en {minutosValidez} minutos.
				     Si no fuiste tú, ignora este correo.</p>
				</div>
				""";
		}
	}
}
