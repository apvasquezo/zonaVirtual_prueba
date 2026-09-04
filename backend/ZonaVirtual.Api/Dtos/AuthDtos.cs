using System.ComponentModel.DataAnnotations;
using ZonaVirtual.Api.Models;

namespace ZonaVirtual.Api.Dtos;

public class CheckUsuarioRequest
{
    [Required] public Perfil Perfil { get; set; }
    [Required] public string Identificador { get; set; } = string.Empty; // identificacion (pagador) o nit/codigo (comercio)
}

public class CheckUsuarioResponse
{
    public bool ExistePersona { get; set; }      // ya existe en comercios/usuarios_pagadores (datos semilla)
    public bool TieneCuenta { get; set; }         // ya tiene usuario/clave creados
    public string? NombreSugerido { get; set; }
}

public class RegistroRequest
{
    [Required] public Perfil Perfil { get; set; }
    [Required] public string Identificador { get; set; } = string.Empty; // identificacion o nit
    [Required, MinLength(3)] public string Nombre { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    // Solo requerido si el perfil es Comercio y el comercio no existe aun en la base
    public string? ComercioDireccion { get; set; }
}

public class LoginRequest
{
    [Required] public Perfil Perfil { get; set; }
    [Required] public string Username { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiracion { get; set; }
    public Perfil Perfil { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int ReferenciaId { get; set; } // UsuarioPagadorId o ComercioId
}
