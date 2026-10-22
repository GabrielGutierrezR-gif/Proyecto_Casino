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
using Casino_API.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Base de datos
// Conecta EF con SQL Server usando la cadena del appsettings.json
builder.Services.AddDbContext<CasinoContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CasinoDB")));

// Inyección de dependencias
// Cuando alguien pide una interfaz, la API entrega la clase que la implementa
builder.Services.AddScoped<IUnidadTrabajoEF, UnidadTrabajoEF>();
builder.Services.AddScoped<IUsuarioLN, UsuarioLN>();
builder.Services.AddScoped<ICompraLN, CompraLN>();
builder.Services.AddScoped<IRuletaLN, RuletaLN>();
builder.Services.AddScoped<ISalaLN, SalaLN>();
// Blackjack en tiempo real: SINGLETON porque guarda las mesas en memoria
builder.Services.AddSingleton<IBlackjackLN, BlackjackLN>();
builder.Services.AddSingleton<INotificadorBlackjack, NotificadorSignalR>();
builder.Services.AddSingleton<IUserIdProvider, UsuarioIdProvider>();
builder.Services.AddSignalR();
builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperProfile));
builder.Services.AddSingleton<TokenService>();

// Autenticación con JWT 
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

        // SignalR manda el token en la URL (?access_token=...) en vez del header
        opciones.Events = new JwtBearerEvents
        {
            OnMessageReceived = contexto =>
            {
                var token = contexto.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token) && contexto.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    contexto.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// Controladores y Swagger
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

// Pipeline (orden en que se procesa cada petición) 
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();   // primero: ¿quién eres?
app.UseAuthorization();    // después: ¿tienes permiso?

app.MapControllers();
app.MapHub<BlackjackHub>("/hubs/blackjack");
// Al arrancar: devolver fichas de rondas que quedaron a medias si la API se apagó 
await app.Services.GetRequiredService<IBlackjackLN>().CancelarRondasPendientesAsync();
app.Run();
