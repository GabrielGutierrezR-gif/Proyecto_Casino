
using Casino_AccesoDatos.Contexto;
using Casino_AccesoDatos.Implementaciones;
using Casino_API.Servicios;
using Casino_Dominio.DTO;
using Casino_Dominio.InterfacesAD;
using Casino_Dominio.InterfacesLN;
using Casino_LogicaNegocios.Implementaciones;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));
builder.Services.AddSingleton<TokenService>();

// ===== 3. CORS: permitir la conexión con Angular/Ionic =====
builder.Services.AddCors(options =>
{
	options.AddPolicy("FrontendCasino", policy =>
	{
		policy
			.WithOrigins("http://localhost:8100")
			.AllowAnyHeader()
			.AllowAnyMethod();
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

app.Run();
