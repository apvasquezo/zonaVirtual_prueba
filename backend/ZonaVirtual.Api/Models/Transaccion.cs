using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZonaVirtual.Api.Models;

public class Transaccion
{
    public int Id { get; set; }

    [Required]
    public long TransCodigo { get; set; } // unico

    [Required]
    public int TransMedioPago { get; set; } // 32,29,41,42

    [Required]
    public int TransEstado { get; set; } // 1,1000,999,1001

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TransTotal { get; set; }

    [Required]
    public DateTime TransFecha { get; set; }

    [MaxLength(300)]
    public string TransConcepto { get; set; } = string.Empty;

    [Required]
    public int ComercioId { get; set; }
    public Comercio Comercio { get; set; } = null!;

    [Required]
    public int UsuarioPagadorId { get; set; }
    public UsuarioPagador UsuarioPagador { get; set; } = null!;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaModificacion { get; set; }
}
