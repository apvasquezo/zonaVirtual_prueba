using Microsoft.AspNetCore.Mvc;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Services;

namespace ZonaVirtual.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("verificar")]
    public async Task<ActionResult<CheckUsuarioResponse>> Verificar([FromBody] CheckUsuarioRequest request)
        => Ok(await _service.VerificarAsync(request));

    [HttpPost("registro")]
    public async Task<ActionResult<LoginResponse>> Registro([FromBody] RegistroRequest request)
    {
        try
        {
            return Ok(await _service.RegistrarAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensaje = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            return Ok(await _service.LoginAsync(request));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { mensaje = ex.Message });
        }
    }
}
