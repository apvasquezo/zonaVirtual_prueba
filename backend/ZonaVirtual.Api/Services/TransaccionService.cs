using Microsoft.EntityFrameworkCore;
using ZonaVirtual.Api.Data;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Services;

public interface ITransaccionService
{
    Task<List<ComercioListItemDto>> ListarComerciosAsync();
    Task<List<TransaccionDto>> ListarPorPagadorAsync(int usuarioPagadorId);
    Task<TransaccionesComercioResponse> ListarPorComercioAsync(int comercioId, BuscarTransaccionesComercioQuery query);
    Task<TransaccionDto> CrearAsync(int usuarioPagadorId, CrearTransaccionRequest request);
    Task<TransaccionDto> ActualizarAsync(int comercioId, int transaccionId, ActualizarTransaccionRequest request);
}

public class TransaccionService : ITransaccionService
{
    private readonly AppDbContext _db;

    public TransaccionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ComercioListItemDto>> ListarComerciosAsync()
    {
        return await _db.Comercios
            .OrderBy(c => c.ComercioNombre)
            .Select(c => new ComercioListItemDto
            {
                Id = c.Id,
                ComercioCodigo = c.ComercioCodigo,
                ComercioNombre = c.ComercioNombre,
                ComercioNit = c.ComercioNit
            })
            .ToListAsync();
    }

    public async Task<List<TransaccionDto>> ListarPorPagadorAsync(int usuarioPagadorId)
    {
        return await _db.Transacciones
            .Include(t => t.Comercio)
            .Include(t => t.UsuarioPagador)
            .Where(t => t.UsuarioPagadorId == usuarioPagadorId)
            .OrderByDescending(t => t.TransFecha)
            .Select(t => MapToDto(t))
            .ToListAsync();
    }

    public async Task<TransaccionesComercioResponse> ListarPorComercioAsync(int comercioId, BuscarTransaccionesComercioQuery query)
    {
        var q = _db.Transacciones
            .Include(t => t.Comercio)
            .Include(t => t.UsuarioPagador)
            .Where(t => t.ComercioId == comercioId);

        if (query.Fecha.HasValue)
            q = q.Where(t => t.TransFecha.Date == query.Fecha.Value.Date);

        if (query.TransCodigo.HasValue)
            q = q.Where(t => t.TransCodigo == query.TransCodigo.Value);

        if (!string.IsNullOrWhiteSpace(query.UsuarioNombre))
            q = q.Where(t => t.UsuarioPagador.UsuarioNombre.Contains(query.UsuarioNombre));

        var lista = await q.OrderByDescending(t => t.TransFecha).ToListAsync();

        return new TransaccionesComercioResponse
        {
            Transacciones = lista.Select(MapToDto).ToList(),
            TotalTransacciones = lista.Sum(t => t.TransTotal)
        };
    }

    public async Task<TransaccionDto> CrearAsync(int usuarioPagadorId, CrearTransaccionRequest request)
    {
        var yaExiste = await _db.Transacciones.AnyAsync(t => t.TransCodigo == request.TransCodigo);
        if (yaExiste)
            throw new InvalidOperationException("Ya existe una transacción con ese código.");

        var comercio = await _db.Comercios.FindAsync(request.ComercioId)
            ?? throw new InvalidOperationException("El comercio seleccionado no existe.");

        var transaccion = new Transaccion
        {
            TransCodigo = request.TransCodigo,
            TransMedioPago = request.TransMedioPago,
            TransEstado = TransEstado.Pendiente,
            TransTotal = request.TransTotal,
            TransFecha = DateTime.UtcNow,
            TransConcepto = request.TransConcepto,
            ComercioId = request.ComercioId,
            UsuarioPagadorId = usuarioPagadorId
        };

        _db.Transacciones.Add(transaccion);
        await _db.SaveChangesAsync();

        await _db.Entry(transaccion).Reference(t => t.Comercio).LoadAsync();
        await _db.Entry(transaccion).Reference(t => t.UsuarioPagador).LoadAsync();

        return MapToDto(transaccion);
    }

    public async Task<TransaccionDto> ActualizarAsync(int comercioId, int transaccionId, ActualizarTransaccionRequest request)
    {
        var transaccion = await _db.Transacciones
            .Include(t => t.Comercio)
            .Include(t => t.UsuarioPagador)
            .FirstOrDefaultAsync(t => t.Id == transaccionId && t.ComercioId == comercioId)
            ?? throw new InvalidOperationException("La transacción no existe o no pertenece a este comercio.");

        if (transaccion.TransEstado == TransEstado.Aprobada)
            throw new InvalidOperationException("No se puede modificar una transacción aprobada.");

        transaccion.TransMedioPago = request.TransMedioPago;
        transaccion.TransEstado = request.TransEstado;
        transaccion.TransTotal = request.TransTotal;
        transaccion.TransConcepto = request.TransConcepto;
        transaccion.FechaModificacion = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(transaccion);
    }

    private static TransaccionDto MapToDto(Transaccion t) => new()
    {
        Id = t.Id,
        TransCodigo = t.TransCodigo,
        TransMedioPago = t.TransMedioPago,
        TransMedioPagoNombre = TransMedioPago.Nombres.GetValueOrDefault(t.TransMedioPago, "Desconocido"),
        TransEstado = t.TransEstado,
        TransEstadoNombre = TransEstado.Nombres.GetValueOrDefault(t.TransEstado, "Desconocido"),
        TransTotal = t.TransTotal,
        TransFecha = t.TransFecha,
        TransConcepto = t.TransConcepto,
        ComercioNombre = t.Comercio.ComercioNombre,
        UsuarioPagadorNombre = t.UsuarioPagador.UsuarioNombre
    };
}
