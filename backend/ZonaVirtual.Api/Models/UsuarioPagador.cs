using System.ComponentModel.DataAnnotations;

namespace ZonaVirtual.Api.Models;

public class UsuarioPagador
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string UsuarioIdentificacion { get; set; } = string.Empty; // usuario_identificacion

    [Required, MaxLength(150)]
    public string UsuarioNombre { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string UsuarioEmail { get; set; } = string.Empty;

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    public Cuenta? Cuenta { get; set; }
}
