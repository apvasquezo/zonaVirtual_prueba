using Microsoft.EntityFrameworkCore;
using ZonaVirtual.Api.Data;
using ZonaVirtual.Api.Dtos;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Services;

public interface IAuthService
{
    Task<CheckUsuarioResponse> VerificarAsync(CheckUsuarioRequest request);
    Task<LoginResponse> RegistrarAsync(RegistroRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwt;

    public AuthService(AppDbContext db, IJwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<CheckUsuarioResponse> VerificarAsync(CheckUsuarioRequest request)
    {
        if (request.Perfil == Perfil.Pagador)
        {
            var usuario = await _db.UsuariosPagadores
                .Include(u => u.Cuenta)
                .FirstOrDefaultAsync(u => u.UsuarioIdentificacion == request.Identificador);

            return new CheckUsuarioResponse
            {
                ExistePersona = usuario != null,
                TieneCuenta = usuario?.Cuenta != null,
                NombreSugerido = usuario?.UsuarioNombre
            };
        }
        else
        {
            var comercio = await _db.Comercios
                .Include(c => c.Cuenta)
                .FirstOrDefaultAsync(c => c.ComercioNit == request.Identificador || c.ComercioCodigo == request.Identificador);

            return new CheckUsuarioResponse
            {
                ExistePersona = comercio != null,
                TieneCuenta = comercio?.Cuenta != null,
                NombreSugerido = comercio?.ComercioNombre
            };
        }
    }

    public async Task<LoginResponse> RegistrarAsync(RegistroRequest request)
    {
        if (request.Perfil == Perfil.Pagador)
        {
            var usuario = await _db.UsuariosPagadores
                .Include(u => u.Cuenta)
                .FirstOrDefaultAsync(u => u.UsuarioIdentificacion == request.Identificador);

            if (usuario == null)
            {
                var emailEnUso = await _db.UsuariosPagadores.AnyAsync(u => u.UsuarioEmail == request.Email);
                if (emailEnUso)
                    throw new InvalidOperationException("Ese correo ya está registrado con otra identificación. Usa uno distinto.");

                usuario = new UsuarioPagador
                {
                    UsuarioIdentificacion = request.Identificador,
                    UsuarioNombre = request.Nombre,
                    UsuarioEmail = request.Email
                };
                _db.UsuariosPagadores.Add(usuario);
                await _db.SaveChangesAsync();
}
            else if (usuario.Cuenta != null)
            {
                throw new InvalidOperationException("Este usuario ya tiene una cuenta creada.");
            }

            var cuenta = new Cuenta
            {
                Perfil = Perfil.Pagador,
                Username = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                UsuarioPagadorId = usuario.Id
            };
            _db.Cuentas.Add(cuenta);
            await _db.SaveChangesAsync();

            var (token, exp) = _jwt.GenerarToken(cuenta.Id, Perfil.Pagador, usuario.Id, usuario.UsuarioNombre);
            return new LoginResponse { Token = token, Expiracion = exp, Perfil = Perfil.Pagador, Nombre = usuario.UsuarioNombre, ReferenciaId = usuario.Id };
        }
        else
        {
            var comercio = await _db.Comercios
                .Include(c => c.Cuenta)
                .FirstOrDefaultAsync(c => c.ComercioNit == request.Identificador || c.ComercioCodigo == request.Identificador);

            if (comercio == null)
            {
                var emailEnUso = await _db.Cuentas.AnyAsync(c => c.Perfil == Perfil.Comercio && c.Username == request.Email);
                if (emailEnUso)
                    throw new InvalidOperationException("Ese correo ya está registrado con otro comercio. Usa uno distinto.");

                comercio = new Comercio
                {
                    ComercioCodigo = request.Identificador,
                    ComercioNombre = request.Nombre,
                    ComercioNit = request.Identificador,
                    ComercioDireccion = request.ComercioDireccion ?? "Sin dirección registrada"
                };
                _db.Comercios.Add(comercio);
                await _db.SaveChangesAsync();
            }
            else if (comercio.Cuenta != null)
            {
                throw new InvalidOperationException("Este comercio ya tiene una cuenta creada.");
            }

            var cuenta = new Cuenta
            {
                Perfil = Perfil.Comercio,
                Username = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password),
                ComercioId = comercio.Id
            };
            _db.Cuentas.Add(cuenta);
            await _db.SaveChangesAsync();

            var (token, exp) = _jwt.GenerarToken(cuenta.Id, Perfil.Comercio, comercio.Id, comercio.ComercioNombre);
            return new LoginResponse { Token = token, Expiracion = exp, Perfil = Perfil.Comercio, Nombre = comercio.ComercioNombre, ReferenciaId = comercio.Id };
        }
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var cuenta = await _db.Cuentas
            .Include(c => c.UsuarioPagador)
            .Include(c => c.Comercio)
            .FirstOrDefaultAsync(c => c.Perfil == request.Perfil && c.Username == request.Username);

        if (cuenta == null || !PasswordHasher.Verify(request.Password, cuenta.PasswordHash))
            throw new UnauthorizedAccessException("Usuario o contraseña incorrectos.");

        var nombre = cuenta.Perfil == Perfil.Pagador ? cuenta.UsuarioPagador!.UsuarioNombre : cuenta.Comercio!.ComercioNombre;
        var referenciaId = cuenta.Perfil == Perfil.Pagador ? cuenta.UsuarioPagadorId!.Value : cuenta.ComercioId!.Value;

        var (token, exp) = _jwt.GenerarToken(cuenta.Id, cuenta.Perfil, referenciaId, nombre);
        return new LoginResponse { Token = token, Expiracion = exp, Perfil = cuenta.Perfil, Nombre = nombre, ReferenciaId = referenciaId };
    }
}
