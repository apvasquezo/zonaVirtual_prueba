using System.ComponentModel.DataAnnotations;

namespace ZonaVirtual.Api.Models;

public class Comercio
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    public string ComercioCodigo { get; set; } = string.Empty; // comercio_codigo

    [Required, MaxLength(150)]
    public string ComercioNombre { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string ComercioNit { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string ComercioDireccion { get; set; } = string.Empty;

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>(); // relacion 1 a muchos
    public Cuenta? Cuenta { get; set; } //puede ser null
}
