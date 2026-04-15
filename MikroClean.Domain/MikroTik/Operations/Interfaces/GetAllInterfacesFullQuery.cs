using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Interfaces;

/// <summary>
/// Consulta completa para obtener todas las interfaces del router con todos los campos para sincronización
/// </summary>
public class GetAllInterfacesFullQuery : IMikroTikQuery<List<InterfaceFullResponse>>
{
    public string Command => "/interface/print";

    public List<InterfaceFullResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var interfaces = new List<InterfaceFullResponse>();

        foreach (var sentence in responses)
        {
            interfaces.Add(new InterfaceFullResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                DefaultName = sentence.GetOptionalField("default-name"),
                Type = sentence.GetResponseField("type"),
                Mtu = int.TryParse(sentence.GetResponseField("mtu"), out var mtu) ? mtu : 1500,
                ActualMtu = int.TryParse(sentence.GetResponseField("actual-mtu"), out var actualMtu) ? actualMtu : 0,
                MaxL2Mtu = int.TryParse(sentence.GetResponseField("max-l2mtu"), out var maxL2Mtu) ? maxL2Mtu : 0,
                MacAddress = sentence.GetResponseField("mac-address"),
                LinkDowns = int.TryParse(sentence.GetResponseField("link-downs"), out var linkDowns) ? linkDowns : 0,
                RxByte = long.TryParse(sentence.GetResponseField("rx-byte"), out var rxByte) ? rxByte : 0,
                TxByte = long.TryParse(sentence.GetResponseField("tx-byte"), out var txByte) ? txByte : 0,
                RxPacket = long.TryParse(sentence.GetResponseField("rx-packet"), out var rxPacket) ? rxPacket : 0,
                TxPacket = long.TryParse(sentence.GetResponseField("tx-packet"), out var txPacket) ? txPacket : 0,
                RxDrop = long.TryParse(sentence.GetResponseField("rx-drop"), out var rxDrop) ? rxDrop : 0,
                TxDrop = long.TryParse(sentence.GetResponseField("tx-drop"), out var txDrop) ? txDrop : 0,
                TxQueueDrop = long.TryParse(sentence.GetResponseField("tx-queue-drop"), out var txQueueDrop) ? txQueueDrop : 0,
                RxError = long.TryParse(sentence.GetResponseField("rx-error"), out var rxError) ? rxError : 0,
                TxError = long.TryParse(sentence.GetResponseField("tx-error"), out var txError) ? txError : 0,
                FpRxByte = long.TryParse(sentence.GetResponseField("fp-rx-byte"), out var fpRxByte) ? fpRxByte : 0,
                FpTxByte = long.TryParse(sentence.GetResponseField("fp-tx-byte"), out var fpTxByte) ? fpTxByte : 0,
                FpRxPacket = long.TryParse(sentence.GetResponseField("fp-rx-packet"), out var fpRxPacket) ? fpRxPacket : 0,
                FpTxPacket = long.TryParse(sentence.GetResponseField("fp-tx-packet"), out var fpTxPacket) ? fpTxPacket : 0,
                Running = sentence.GetResponseField("running") == "true",
                Disabled = sentence.GetResponseField("disabled") == "true",
                Comment = sentence.GetOptionalField("comment")
            });
        }

        return interfaces;
    }
}

/// <summary>
/// Respuesta completa de una interfaz con todos los campos para sincronización
/// </summary>
public class InterfaceFullResponse
{
    public string Id { get; set; } = string.Empty;
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
}
