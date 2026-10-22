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
// Conecta EF con SQL Server usando la cadena del appsettings.json
builder.Services.AddDbContext<CasinoContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CasinoDB")));

// ===== 2. Inyección de dependencias =====
// Cuando alguien pide una interfaz, la API entrega la clase que la implementa
builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();
builder.Services.AddScoped<IUsuarioLN, UsuarioLN>();
builder.Services.AddScoped<ICompraLN, CompraLN>();
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));
builder.Services.AddSingleton<TokenService>();

// ===== 3. Autenticación con JWT (RNF6) =====
// Revisa que el token de cada petición sea válido
var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,               // creado por nuestra API
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,             // para nuestra app
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,             // que no esté vencido
            ValidateIssuerSigningKey = true,     // firmado con nuestra clave
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// ===== 4. Controladores y Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Casino API", Version = "v1" });

    // Botón "Authorize" en Swagger para probar con token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega aquí el token (sin la palabra Bearer)"
    });
    c.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
    });
});

var app = builder.Build();

// ===== 5. Pipeline (orden en que se procesa cada petición) =====
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();   // primero: ¿quién eres?
app.UseAuthorization();    // después: ¿tienes permiso?

app.MapControllers();
app.Run();
