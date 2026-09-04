using System.ComponentModel.DataAnnotations;

namespace ZonaVirtual.Api.Models;

// Credenciales de acceso. Cada cuenta pertenece a un pagador o a un comercio, nunca a ambos.
public class Cuenta
{
    public int Id { get; set; }

    [Required]
    public Perfil Perfil { get; set; }

    [Required, MaxLength(150)]
    public string Username { get; set; } = string.Empty; // email

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

public static class TransEstado
{
    public const int Aprobada = 1;
    public const int Rechazada = 1000;
    public const int Pendiente = 999;
    public const int RechazadaSR = 1001;

    public static readonly Dictionary<int, string> Nombres = new()
    {
        { Aprobada, "Aprobada" },
        { Rechazada, "Rechazada" },
        { Pendiente, "Pendiente" },
        { RechazadaSR, "Rechazada SR" }
    };
}

public static class TransMedioPago
{
    public const int TarjetaCredito = 32;
    public const int PSE = 29;
    public const int Gana = 41;
    public const int Caja = 42;

    public static readonly Dictionary<int, string> Nombres = new()
    {
        { TarjetaCredito, "Tarjeta de Crédito" },
        { PSE, "PSE" },
        { Gana, "Gana" },
        { Caja, "Caja" }
    };
}
