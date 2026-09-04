using System.ComponentModel.DataAnnotations;

namespace ZonaVirtual.Api.Dtos;

public class ComercioListItemDto
{
    public int Id { get; set; }
    public string ComercioCodigo { get; set; } = string.Empty;
    public string ComercioNombre { get; set; } = string.Empty;
    public string ComercioNit { get; set; } = string.Empty;
}

public class CrearTransaccionRequest
{
    [Required] public long TransCodigo { get; set; }
    [Required] public int TransMedioPago { get; set; }
    [Required] public int ComercioId { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal TransTotal { get; set; }
    [Required] public string TransConcepto { get; set; } = string.Empty;
}

public class ActualizarTransaccionRequest
{
    [Required] public int TransMedioPago { get; set; }
    [Required] public int TransEstado { get; set; }
    [Required, Range(0.01, double.MaxValue)] public decimal TransTotal { get; set; }
    [Required] public string TransConcepto { get; set; } = string.Empty;
}

public class TransaccionDto
{
    public int Id { get; set; }
    public long TransCodigo { get; set; }
    public int TransMedioPago { get; set; }
    public string TransMedioPagoNombre { get; set; } = string.Empty;
    public int TransEstado { get; set; }
    public string TransEstadoNombre { get; set; } = string.Empty;
    public decimal TransTotal { get; set; }
    public DateTime TransFecha { get; set; }
    public string TransConcepto { get; set; } = string.Empty;
    public string ComercioNombre { get; set; } = string.Empty;
    public string UsuarioPagadorNombre { get; set; } = string.Empty;
}

public class TransaccionesComercioResponse
{
    public List<TransaccionDto> Transacciones { get; set; } = new();
    public decimal TotalTransacciones { get; set; }
}

public class BuscarTransaccionesComercioQuery
{
    public DateTime? Fecha { get; set; }
    public long? TransCodigo { get; set; }
    public string? UsuarioNombre { get; set; }
}
