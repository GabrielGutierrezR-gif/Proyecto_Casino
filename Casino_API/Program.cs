
using Casino_AccesoDatos.Contexto;
using Casino_AccesoDatos.Implementaciones;
using Casino_API.Hubs;
using Casino_API.Servicios;
using Casino_Dominio.DTO;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_LogicaNegocios.Implementaciones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== 1. Base de datos =====
builder.Services.AddDbContext<CasinoContext>(opciones =>
	opciones.UseSqlServer(
		builder.Configuration.GetConnectionString("CasinoDB")));

// ===== 2. Inyección de dependencias =====
builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();
builder.Services.AddScoped<IUsuarioLN, UsuarioLN>();
builder.Services.AddScoped<ICompraLN, CompraLN>();
builder.Services.AddScoped<IRuletaLN, RuletaLN>();
builder.Services.AddScoped<ISalaLN, SalaLN>();
builder.Services.AddScoped<IConfiguracionLN, ConfiguracionLN>();
builder.Services.AddScoped<IPaProcedimientoAD, PaProcedimientoAD>();
builder.Services.AddScoped<IHistorialLN, HistorialLN>();

// Verificación de correo al crear la cuenta: los códigos viven en memoria
builder.Services.AddSingleton<IVerificacionCorreoLN, VerificacionCorreoLN>();
builder.Services.AddSingleton<IEnvioCorreo, EmailService>();

// Blackjack: las mesas viven en memoria, por eso la lógica y el notificador son SINGLETON
builder.Services.AddSingleton<INotificadorBlackjack, NotificadorSignalR>();
builder.Services.AddSingleton<IBlackjackLN, BlackjackLN>();

builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));
builder.Services.AddSingleton<TokenService>();

// SignalR: tiempo real del Blackjack. El "usuario" de cada conexión es el "sub" del token.
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UsuarioIdProvider>();

// ===== 3. CORS: permitir la conexión con Angular/Ionic =====
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendCasino", policy =>
	{
		// 8100: ionic serve · 4200: ng serve · capacitor/localhost: app instalada en el teléfono
		policy
			.WithOrigins(
				"http://localhost:8100",
				"http://localhost:4200",
				"http://localhost",
				"https://localhost",
				"capacitor://localhost")
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();   // lo exige SignalR
	});
});

// ===== 4. Autenticación con JWT =====
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(opciones =>
	{
		opciones.MapInboundClaims = false;

		opciones.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwt["Issuer"],

			ValidateAudience = true,
			ValidAudience = jwt["Audience"],

			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,

			IssuerSigningKey = new SymmetricSecurityKey(
				Encoding.UTF8.GetBytes(jwt["Key"]!)),

			NameClaimType = "name",
			RoleClaimType = "role"
		};

		// Los WebSockets no pueden mandar el header Authorization:
		// SignalR envía el token en la URL (?access_token=...) y aquí se lee.
		opciones.Events = new JwtBearerEvents
		{
			OnMessageReceived = contexto =>
			{
				var token = contexto.Request.Query["access_token"];

				if (!string.IsNullOrEmpty(token) &&
					contexto.HttpContext.Request.Path.StartsWithSegments("/hubs"))
				{
					contexto.Token = token;
				}

				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorization();

// ===== 5. Controladores y Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Casino API",
		Version = "v1"
	});

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Type = SecuritySchemeType.Http,
		Scheme = "bearer",
		BearerFormat = "JWT",
		Description = "Pega aquí el token (sin la palabra Bearer)"
	});

	c.AddSecurityRequirement(documento =>
		new OpenApiSecurityRequirement
		{
			[new OpenApiSecuritySchemeReference("Bearer", documento)] = []
		});
});

var app = builder.Build();

// ===== 6. Pipeline =====
app.UseSwagger();
app.UseSwaggerUI();

// CORS debe ejecutarse antes de autenticación y autorización.
app.UseCors("FrontendCasino");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<BlackjackHub>("/hubs/blackjack");

// Si la API se apagó a mitad de una ronda, se devuelven esas fichas a los jugadores
await app.Services.GetRequiredService<IBlackjackLN>().CancelarRondasPendientesAsync();

app.Run();
