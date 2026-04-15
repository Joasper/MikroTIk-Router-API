using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE;

/// <summary>
/// Consulta para obtener todas las conexiones PPPoE activas del router
/// </summary>
public class GetAllPPPoEActiveConnectionsQuery : IMikroTikQuery<List<PPPoEActiveConnectionResponse>>
{
    public string Command => "/ppp/active/print";
    
    public List<PPPoEActiveConnectionResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var connections = new List<PPPoEActiveConnectionResponse>();
        
        foreach (var sentence in responses)
        {
            connections.Add(new PPPoEActiveConnectionResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                Service = sentence.GetResponseField("service"),
                CallerId = sentence.GetResponseField("caller-id"),
                Address = sentence.GetResponseField("address"),
                Uptime = sentence.GetResponseField("uptime")
            });
        }
        
        return connections;
    }
}
