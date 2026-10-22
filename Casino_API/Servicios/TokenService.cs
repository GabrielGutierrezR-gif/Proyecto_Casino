using Casino_Dominio.Clases;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Casino_API.Servicios
{
    // Genera el token JWT que la app guarda para no pedir la contraseña cada vez 
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerarToken(clsPerfilUsuario usuario)
        {
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            int dias = int.Parse(_config["Jwt:DiasExpiracion"] ?? "7");

            // Datos que viajan dentro del token
            var claims = new List<Claim>
            {
                new Claim("sub", usuario.IdUsuario.ToString()),
                new Claim("name", usuario.NombreUsuario),
                new Claim("email", usuario.Correo),
                new Claim("role", usuario.Rol)
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(dias),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
            };

            return new JsonWebTokenHandler().CreateToken(descriptor);
        }
    }
}