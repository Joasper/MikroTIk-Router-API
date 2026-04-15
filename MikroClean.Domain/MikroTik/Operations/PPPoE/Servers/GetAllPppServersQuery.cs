using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;

/// <summary>
/// Consulta para obtener todos los servidores PPPoE del router
/// </summary>
public class GetAllPppServersQuery : IMikroTikQuery<List<PPPoEServerResponse>>
{
    public string Command => "/interface/pppoe-server/server/print";
    
    public List<PPPoEServerResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var servers = new List<PPPoEServerResponse>();
        
        foreach (var sentence in responses)
        {
            servers.Add(new PPPoEServerResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("service-name"),
                Interface = sentence.GetResponseField("interface"),
                Profile = sentence.GetResponseField("default-profile"),
                MaxMTU = sentence.GetResponseField("max-mtu"),
                MaxMRU = sentence.GetResponseField("max-mru"),
                KeepAliveTimeOut = sentence.GetResponseField("keepalive-timeout"),
                OneSesionPerHost = sentence.GetResponseField("one-session-per-host"),
                Comment = sentence.GetOptionalField("comment")
            });
        }
        
        return servers;
    }
}
