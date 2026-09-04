using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Services;

public interface IJwtService
{
    (string token, DateTime expiracion) GenerarToken(int cuentaId, Perfil perfil, int referenciaId, string nombre);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiracion) GenerarToken(int cuentaId, Perfil perfil, int referenciaId, string nombre)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expireMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "120");
        var expiracion = DateTime.UtcNow.AddMinutes(expireMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, cuentaId.ToString()),
            new Claim(ClaimTypes.Role, perfil.ToString()),
            new Claim("referenciaId", referenciaId.ToString()),
            new Claim(ClaimTypes.Name, nombre)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiracion,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}
