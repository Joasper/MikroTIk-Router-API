using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;

/// <summary>
/// Operación para obtener un servidor PPPoE específico por su ID
/// </summary>
public class GetPppServerByIdOperation : IMikroTikOperation<string, PPPoEServerResponse?>
{
    public string Command => "/interface/pppoe-server/server/print";
    
    public Dictionary<string, string> BuildParameters(string id)
    {
        return new Dictionary<string, string>
        {
            ["?.id"] = id
        };
    }
    
    public PPPoEServerResponse? ParseResponse(ITikSentence response)
    {
        if (response == null) return null;
        
        return new PPPoEServerResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("service-name"),
            Interface = response.GetResponseField("interface"),
            Profile = response.GetResponseField("default-profile"),
            MaxMTU = response.GetResponseField("max-mtu"),
            MaxMRU = response.GetResponseField("max-mru"),
            KeepAliveTimeOut = response.GetResponseField("keepalive-timeout"),
            OneSesionPerHost = response.GetResponseField("one-session-per-host"),
            Comment = response.GetOptionalField("comment")
        };
    }
}
