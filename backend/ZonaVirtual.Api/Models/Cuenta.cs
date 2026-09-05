using System.ComponentModel.DataAnnotations;

namespace ZonaVirtual.Api.Models;

// Credenciales de acceso. Cada cuenta pertenece a un pagador o a un comercio, nunca a ambos.
public class Cuenta
{
    public int Id { get; set; }

    [Required]
    public Perfil Perfil { get; set; }

    [Required, MaxLength(150)]
    public string Username { get; set; } = string.Empty; // email o nit/identificacion segun perfil

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public int? UsuarioPagadorId { get; set; }
    public UsuarioPagador? UsuarioPagador { get; set; }

    public int? ComercioId { get; set; }
    public Comercio? Comercio { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public enum Perfil
{
    Pagador = 1,
    Comercio = 2
}
