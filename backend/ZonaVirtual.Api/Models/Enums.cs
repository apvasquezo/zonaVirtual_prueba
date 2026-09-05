namespace ZonaVirtual.Api.Models;

public static class TransEstado
{
    public const int Aprobada = 1;
    public const int Rechazada = 1000;
    public const int Pendiente = 999;
    public const int RechazadaSR = 1001;

    public static readonly Dictionary<int, string> Nombres = new()
    {
        { Aprobada, "Aprobada" },
        { Rechazada, "Rechazada" },
        { Pendiente, "Pendiente" },
        { RechazadaSR, "Rechazada SR" }
    };
}

public static class TransMedioPago
{
    public const int TarjetaCredito = 32;
    public const int PSE = 29;
    public const int Gana = 41;
    public const int Caja = 42;

    public static readonly Dictionary<int, string> Nombres = new()
    {
        { TarjetaCredito, "Tarjeta de Crédito" },
        { PSE, "PSE" },
        { Gana, "Gana" },
        { Caja, "Caja" }
    };
}