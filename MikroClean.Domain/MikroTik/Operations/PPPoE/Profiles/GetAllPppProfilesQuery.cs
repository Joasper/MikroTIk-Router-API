using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;

/// <summary>
/// Consulta para obtener todos los perfiles PPPoE del router
/// </summary>
public class GetAllPppProfilesQuery : IMikroTikQuery<List<PPPoEProfileResponse>>
{
    public string Command => "/ppp/profile/print";
    
    public List<PPPoEProfileResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var profiles = new List<PPPoEProfileResponse>();
        
        foreach (var sentence in responses)
        {
            profiles.Add(new PPPoEProfileResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                LocalAddress = sentence.GetOptionalField("local-address"),
                RemoteAddress = sentence.GetOptionalField("remote-address"),
                DnsServers = sentence.GetOptionalField("dns-server"),
                RateLimit = sentence.GetOptionalField("rate-limit"),
                OnlyOne = sentence.GetOptionalField("only-one"),
                Comment = sentence.GetOptionalField("comment")
            });
        }
        
        return profiles;
    }
}
