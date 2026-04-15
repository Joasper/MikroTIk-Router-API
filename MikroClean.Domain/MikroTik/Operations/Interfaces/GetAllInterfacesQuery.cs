using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Interfaces;

/// <summary>
/// Consulta para obtener todas las interfaces del router
/// </summary>
public class GetAllInterfacesQuery : IMikroTikQuery<List<InterfaceResponse>>
{
    public string Command => "/interface/print";

    public List<InterfaceResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var interfaces = new List<InterfaceResponse>();

        foreach (var sentence in responses)
        {
            var fields = sentence.GetAllFields();
            Console.WriteLine("Parsing interface: " + string.Join(", ", fields.Select(kv => kv.Key + "=" + kv.Value)));
            interfaces.Add(new InterfaceResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                Type = sentence.GetResponseField("type"),
                MacAddress = sentence.GetResponseField("mac-address"),
                Running = sentence.GetResponseField("running") == "true",
                RxBytes = long.TryParse(sentence.GetResponseField("rx-byte"), out var rx) ? rx : 0,
                TxBytes = long.TryParse(sentence.GetResponseField("tx-byte"), out var tx) ? tx : 0,
                Comment = sentence.GetOptionalField("comment")
            });
        }

        return interfaces;
    }
}
