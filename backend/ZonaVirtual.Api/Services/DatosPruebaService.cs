using Microsoft.EntityFrameworkCore;
using ZonaVirtual.Api.Data;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Services;

public interface IDatosPruebaService
{
    Task<GenerarDatosResponse> GenerarAsync(GenerarDatosRequest request);
}

public class DatosPruebaService : IDatosPruebaService
{
    private readonly AppDbContext _db;
    private static readonly Random _rng = new();

    private static readonly string[] NombresComercio = { "Tienda El Roble", "Supermercado La 80", "Farmacia Vitalis", "Ferreteria Central", "Panaderia San Jose", "Restaurante Sabor", "Papeleria Andina", "Tech Store", "Moda Urbana", "Cafe del Parque" };
    private static readonly string[] NombresPersona = { "Carlos Ramirez", "Maria Gomez", "Andres Lopez", "Laura Torres", "Juan Perez", "Camila Ruiz", "Diego Herrera", "Valentina Diaz", "Santiago Moreno", "Isabela Castro" };
    private static readonly int[] MediosPago = { 32, 29, 41, 42 };
    private static readonly int[] Estados = { 1, 1000, 999, 1001 };

    public DatosPruebaService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GenerarDatosResponse> GenerarAsync(GenerarDatosRequest request)
    {
        var comercios = new List<Comercio>();
        for (int i = 0; i < request.CantidadComercios; i++)
        {
            var codigo = await GenerarCodigoUnicoAsync(async c => await _db.Comercios.AnyAsync(x => x.ComercioCodigo == c) || comercios.Any(x => x.ComercioCodigo == c));
            comercios.Add(new Comercio
            {
                ComercioCodigo = codigo,
                ComercioNombre = NombresComercio[_rng.Next(NombresComercio.Length)] + " " + _rng.Next(1, 999),
                ComercioNit = "NIT" + _rng.Next(100000000, 999999999),
                ComercioDireccion = $"Calle {_rng.Next(1, 150)} # {_rng.Next(1, 90)}-{_rng.Next(1, 99)}"
            });
        }
        _db.Comercios.AddRange(comercios);

        var usuarios = new List<UsuarioPagador>();
        for (int i = 0; i < request.CantidadUsuarios; i++)
        {
            var identificacion = await GenerarCodigoUnicoAsync(async c => await _db.UsuariosPagadores.AnyAsync(x => x.UsuarioIdentificacion == c) || usuarios.Any(x => x.UsuarioIdentificacion == c));
            var nombre = NombresPersona[_rng.Next(NombresPersona.Length)];
            usuarios.Add(new UsuarioPagador
            {
                UsuarioIdentificacion = identificacion,
                UsuarioNombre = nombre,
                UsuarioEmail = nombre.ToLower().Replace(" ", ".") + _rng.Next(1, 999) + "@correo.com"
            });
        }
        _db.UsuariosPagadores.AddRange(usuarios);

        await _db.SaveChangesAsync(); // aseguramos Ids antes de crear transacciones

        var transacciones = new List<Transaccion>();
        for (int i = 0; i < request.CantidadTransacciones; i++)
        {
            if (comercios.Count == 0 || usuarios.Count == 0) break;

            long transCodigo;
            do
            {
                transCodigo = long.Parse(DateTime.UtcNow.Ticks.ToString().Substring(10)) + _rng.Next(1, 999999);
            } while (await _db.Transacciones.AnyAsync(t => t.TransCodigo == transCodigo) || transacciones.Any(t => t.TransCodigo == transCodigo));

            transacciones.Add(new Transaccion
            {
                TransCodigo = transCodigo,
                TransMedioPago = MediosPago[_rng.Next(MediosPago.Length)],
                TransEstado = Estados[_rng.Next(Estados.Length)],
                TransTotal = Math.Round((decimal)(_rng.NextDouble() * 490000 + 10000), 2),
                TransFecha = DateTime.UtcNow.AddDays(-_rng.Next(0, 60)),
                TransConcepto = "Pago de prueba #" + _rng.Next(1000, 9999),
                ComercioId = comercios[_rng.Next(comercios.Count)].Id,
                UsuarioPagadorId = usuarios[_rng.Next(usuarios.Count)].Id
            });
        }
        _db.Transacciones.AddRange(transacciones);
        await _db.SaveChangesAsync();

        return new GenerarDatosResponse
        {
            ComerciosCreados = comercios.Count,
            UsuariosCreados = usuarios.Count,
            TransaccionesCreadas = transacciones.Count
        };
    }

    private async Task<string> GenerarCodigoUnicoAsync(Func<string, Task<bool>> existe)
    {
        string codigo;
        do
        {
            codigo = _rng.Next(100000, 999999).ToString();
        } while (await existe(codigo));
        return codigo;
    }
}
