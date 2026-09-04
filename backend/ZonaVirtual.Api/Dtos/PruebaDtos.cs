namespace ZonaVirtual.Api.Dtos;

public class GenerarDatosRequest
{
    public int CantidadComercios { get; set; } = 5;
    public int CantidadUsuarios { get; set; } = 10;
    public int CantidadTransacciones { get; set; } = 25;
}

public class GenerarDatosResponse
{
    public int ComerciosCreados { get; set; }
    public int UsuariosCreados { get; set; }
    public int TransaccionesCreadas { get; set; }
}
