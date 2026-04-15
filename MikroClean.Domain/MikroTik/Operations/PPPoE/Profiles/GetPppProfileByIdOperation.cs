using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;

/// <summary>
/// Operación para obtener un perfil PPPoE específico por su ID
/// </summary>
public class GetPppProfileByIdOperation : IMikroTikOperation<string, PPPoEProfileResponse?>
{
    public string Command => "/ppp/profile/print";
    
    public Dictionary<string, string> BuildParameters(string id)
    {
        return new Dictionary<string, string>
        {
            ["?.id"] = id
        };
    }
    
    public PPPoEProfileResponse? ParseResponse(ITikSentence response)
    {
        if (response == null) return null;
        
        return new PPPoEProfileResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            LocalAddress = response.GetResponseField("local-address"),
            RemoteAddress = response.GetResponseField("remote-address"),
            DnsServers = response.GetResponseField("dns-server"),
            RateLimit = response.GetResponseField("rate-limit"),
            OnlyOne = response.GetResponseField("only-one"),
            Comment = response.GetOptionalField("comment")
        };
    }
}
