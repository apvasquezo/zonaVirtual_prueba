using Microsoft.AspNetCore.Mvc;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Services;

namespace ZonaVirtual.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PruebaController : ControllerBase
{
    private readonly IDatosPruebaService _service;

    public PruebaController(IDatosPruebaService service)
    {
        _service = service;
    }

    // POST /api/Prueba/GenerarDatos
    [HttpPost("GenerarDatos")]
    public async Task<ActionResult<GenerarDatosResponse>> GenerarDatos([FromBody] GenerarDatosRequest? request)
    {
        request ??= new GenerarDatosRequest();
        var resultado = await _service.GenerarAsync(request);
        return Ok(resultado);
    }
}
