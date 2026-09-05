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
        // Traemos una sola vez los codigos/identificaciones que ya existen en la base
        var codigosComercioUsados = new HashSet<string>(await _db.Comercios.Select(c => c.ComercioCodigo).ToListAsync());
        var identificacionesUsadas = new HashSet<string>(await _db.UsuariosPagadores.Select(u => u.UsuarioIdentificacion).ToListAsync());
        var transCodigosUsados = new HashSet<long>(await _db.Transacciones.Select(t => t.TransCodigo).ToListAsync());

        var comercios = new List<Comercio>();
        for (int i = 0; i < request.CantidadComercios; i++)
        {
            var codigo = GenerarUnico(codigosComercioUsados, () => _rng.Next(100_000, 999_999).ToString());
            comercios.Add(new Comercio
            {
                ComercioCodigo = codigo,
                ComercioNombre = NombresComercio[_rng.Next(NombresComercio.Length)] + " " + _rng.Next(1, 999),
                ComercioNit = _rng.Next(100_000_000, 999_999_999).ToString(),
                ComercioDireccion = $"Calle {_rng.Next(1, 150)} # {_rng.Next(1, 90)}-{_rng.Next(1, 99)}"
            });
        }
        _db.Comercios.AddRange(comercios);

        var usuarios = new List<UsuarioPagador>();
        for (int i = 0; i < request.CantidadUsuarios; i++)
        {
            var identificacion = GenerarUnico(identificacionesUsadas, () => _rng.Next(100_000, 999_999).ToString());
            var nombre = NombresPersona[_rng.Next(NombresPersona.Length)];
            usuarios.Add(new UsuarioPagador
            {
                UsuarioIdentificacion = identificacion,
                UsuarioNombre = nombre,
                UsuarioEmail = nombre.ToLower().Replace(" ", ".") + _rng.Next(1, 999) + "@correo.com"
            });
        }
        _db.UsuariosPagadores.AddRange(usuarios);

        // Guardamos comercios y usuarios antes de crear transacciones, porque estas
        // necesitan sus Ids reales (generados por la base) para las llaves foraneas.
        await _db.SaveChangesAsync();

        var transacciones = new List<Transaccion>();
        if (comercios.Count > 0 && usuarios.Count > 0)
        {
            for (int i = 0; i < request.CantidadTransacciones; i++)
            {
                var transCodigo = GenerarUnico(transCodigosUsados, () => _rng.NextInt64(100_000_000, 999_999_999));

                transacciones.Add(new Transaccion
                {
                    TransCodigo = transCodigo,
                    TransMedioPago = MediosPago[_rng.Next(MediosPago.Length)],
                    TransEstado = Estados[_rng.Next(Estados.Length)],
                    TransTotal = Math.Round((decimal)(_rng.NextDouble() * 490_000 + 10_000), 2),
                    TransFecha = DateTime.UtcNow.AddDays(-_rng.Next(0, 60)),
                    TransConcepto = "Pago de prueba #" + _rng.Next(1000, 9999),
                    ComercioId = comercios[_rng.Next(comercios.Count)].Id,
                    UsuarioPagadorId = usuarios[_rng.Next(usuarios.Count)].Id
                });
            }
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

    // HashSet.Add devuelve false si el valor ya existia (y no lo vuelve a agregar).
    private static string GenerarUnico(HashSet<string> usados, Func<string> generador)
    {
        string candidato;
        do { candidato = generador(); }
        while (!usados.Add(candidato));
        return candidato;
    }

    private static long GenerarUnico(HashSet<long> usados, Func<long> generador)
    {
        long candidato;
        do { candidato = generador(); }
        while (!usados.Add(candidato));
        return candidato;
    }
}