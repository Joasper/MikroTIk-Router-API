using MikroClean.Domain.Entities;
using MikroClean.Domain.Enums;

namespace MikroClean.Application.Dtos.Interfaces;

/// <summary>
/// DTO para lectura de interfaces de router
/// </summary>
public class RouterInterfaceDTO
{
    public int Id { get; set; }
    public int RouterId { get; set; }
    public string MikroTikId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DefaultName { get; set; }
    public string Type { get; set; } = string.Empty;
    public int Mtu { get; set; }
    public int ActualMtu { get; set; }
    public int MaxL2Mtu { get; set; }
    public string MacAddress { get; set; } = string.Empty;
    public int LinkDowns { get; set; }
    public long RxByte { get; set; }
    public long TxByte { get; set; }
    public long RxPacket { get; set; }
    public long TxPacket { get; set; }
    public long RxDrop { get; set; }
    public long TxDrop { get; set; }
    public long TxQueueDrop { get; set; }
    public long RxError { get; set; }
    public long TxError { get; set; }
    public long FpRxByte { get; set; }
    public long FpTxByte { get; set; }
    public long FpRxPacket { get; set; }
    public long FpTxPacket { get; set; }
    public bool Running { get; set; }
    public bool Disabled { get; set; }
    public string? Comment { get; set; }
    public SyncState SyncState { get; set; }
}

/// <summary>
/// DTO para actualizar una interfaz de router
/// </summary>
public class UpdateRouterInterfaceDTO
{
    /// <summary>
    /// ID de MikroTik de la interfaz a actualizar
    /// </summary>
    public string MikroTikId { get; set; } = string.Empty;

    /// <summary>
    /// Nuevo nombre de la interfaz (opcional)
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// MTU a configurar (opcional)
    /// </summary>
    public int? Mtu { get; set; }

    /// <summary>
    /// Habilitar o deshabilitar la interfaz (opcional)
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    /// Comentario personalizado (opcional)
    /// </summary>
    public string? Comment { get; set; }
}

/// <summary>
/// DTO para respuesta de operaciones de interfaces
/// </summary>
public class RouterInterfaceResponse
{
    public string MikroTikId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Running { get; set; }
    public bool Disabled { get; set; }
    public string? Comment { get; set; }
    public SyncState SyncState { get; set; }
}
