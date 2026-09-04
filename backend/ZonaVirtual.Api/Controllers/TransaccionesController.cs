using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Services;

namespace ZonaVirtual.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionService _service;

    public TransaccionesController(ITransaccionService service)
    {
        _service = service;
    }

    private int ReferenciaId => int.Parse(User.FindFirstValue("referenciaId")!);
    private string PerfilActual => User.FindFirstValue(ClaimTypes.Role)!;

    [HttpGet("comercios")]
    public async Task<ActionResult<List<ComercioListItemDto>>> ListarComercios()
        => Ok(await _service.ListarComerciosAsync());

    [HttpGet("pagador")]
    [Authorize(Roles = "Pagador")]
    public async Task<ActionResult<List<TransaccionDto>>> ListarPorPagador()
        => Ok(await _service.ListarPorPagadorAsync(ReferenciaId));

    [HttpPost("pagador")]
    [Authorize(Roles = "Pagador")]
    public async Task<ActionResult<TransaccionDto>> Crear([FromBody] CrearTransaccionRequest request)
    {
        try
        {
            return Ok(await _service.CrearAsync(ReferenciaId, request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpGet("comercio")]
    [Authorize(Roles = "Comercio")]
    public async Task<ActionResult<TransaccionesComercioResponse>> ListarPorComercio(
        [FromQuery] DateTime? fecha, [FromQuery] long? transCodigo, [FromQuery] string? usuarioNombre)
    {
        var query = new BuscarTransaccionesComercioQuery { Fecha = fecha, TransCodigo = transCodigo, UsuarioNombre = usuarioNombre };
        return Ok(await _service.ListarPorComercioAsync(ReferenciaId, query));
    }

    [HttpPut("comercio/{id:int}")]
    [Authorize(Roles = "Comercio")]
    public async Task<ActionResult<TransaccionDto>> Actualizar(int id, [FromBody] ActualizarTransaccionRequest request)
    {
        try
        {
            return Ok(await _service.ActualizarAsync(ReferenciaId, id, request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }
}
