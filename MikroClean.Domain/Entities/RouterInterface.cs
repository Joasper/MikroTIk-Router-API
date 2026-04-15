using MikroClean.Domain.Entities.Base;

namespace MikroClean.Domain.Entities;

/// <summary>
/// Representa una interfaz de red de un router MikroTik
/// Almacena información de interfaces (ether, vlan, bridge, wireless, etc.)
/// </summary>
public class RouterInterface : BaseEntity
{
    /// <summary>
    /// ID del router al que pertenece esta interfaz
    /// </summary>
    public int RouterId { get; set; }

    /// <summary>
    /// ID interno de MikroTik (ej: "*3")
    /// </summary>
    public string MikroTikId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la interfaz (ej: "ether1", "bridge-local")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Nombre por defecto de fábrica (ej: "ether1")
    /// </summary>
    public string? DefaultName { get; set; }

    /// <summary>
    /// Tipo de interfaz (ether, vlan, bridge, wireless, pppoe, etc.)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// MTU configurada (Maximum Transmission Unit)
    /// </summary>
    public int Mtu { get; set; } = 1500;

    /// <summary>
    /// MTU actual real de la interfaz
    /// </summary>
    public int ActualMtu { get; set; }

    /// <summary>
    /// MTU máxima soportada a nivel 2
    /// </summary>
    public int MaxL2Mtu { get; set; }

    /// <summary>
    /// Dirección MAC de la interfaz
    /// </summary>
    public string MacAddress { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de veces que el link ha caído
    /// </summary>
    public int LinkDowns { get; set; }

    /// <summary>
    /// Bytes recibidos (contador acumulado)
    /// </summary>
    public long RxByte { get; set; }

    /// <summary>
    /// Bytes transmitidos (contador acumulado)
    /// </summary>
    public long TxByte { get; set; }

    /// <summary>
    /// Paquetes recibidos
    /// </summary>
    public long RxPacket { get; set; }

    /// <summary>
    /// Paquetes transmitidos
    /// </summary>
    public long TxPacket { get; set; }

    /// <summary>
    /// Paquetes recibidos descartados
    /// </summary>
    public long RxDrop { get; set; }

    /// <summary>
    /// Paquetes transmitidos descartados
    /// </summary>
    public long TxDrop { get; set; }

    /// <summary>
    /// Paquetes descartados en cola de transmisión
    /// </summary>
    public long TxQueueDrop { get; set; }

    /// <summary>
    /// Errores de recepción
    /// </summary>
    public long RxError { get; set; }

    /// <summary>
    /// Errores de transmisión
    /// </summary>
    public long TxError { get; set; }

    /// <summary>
    /// Bytes recibidos por fastpath
    /// </summary>
    public long FpRxByte { get; set; }

    /// <summary>
    /// Bytes transmitidos por fastpath
    /// </summary>
    public long FpTxByte { get; set; }

    /// <summary>
    /// Paquetes recibidos por fastpath
    /// </summary>
    public long FpRxPacket { get; set; }

    /// <summary>
    /// Paquetes transmitidos por fastpath
    /// </summary>
    public long FpTxPacket { get; set; }

    /// <summary>
    /// Indica si la interfaz está en funcionamiento (running)
    /// </summary>
    public bool Running { get; set; }

    /// <summary>
    /// Indica si la interfaz está deshabilitada
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Comentario personalizado de la interfaz
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Estado de sincronización con el router
    /// </summary>
    public Domain.Enums.SyncState SyncState { get; set; } = Domain.Enums.SyncState.Synced;

    // Navegación
    /// <summary>
    /// Router al que pertenece esta interfaz
    /// </summary>
    public Router? Router { get; set; }
}
