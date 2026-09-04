using System.Text.Json;

namespace ZonaVirtual.Api.Middleware;

// Captura cualquier excepcion no manejada por los controllers (fallos de conexion a la base,
// errores inesperados, etc.) y devuelve una respuesta JSON limpia en vez de exponer el stack
// trace al cliente. Los errores de negocio esperados (InvalidOperationException, etc.) ya se
// manejan dentro de cada controller con try/catch puntuales; este middleware es la ultima
// linea de defensa para todo lo demas.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            string mensaje = _env.IsDevelopment()
                ? "Ocurrio un error inesperado."
                : "Ocurrio un error inesperado. Intenta nuevamente mas tarde.";

            string? detalle = _env.IsDevelopment() ? ex.Message : null;

            var body = new { mensaje, detalle };
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}