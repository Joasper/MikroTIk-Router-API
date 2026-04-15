using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;

/// <summary>
/// Operación para crear un nuevo perfil PPPoE en el router
/// </summary>
public class CreatePppProfileOperation : IMikroTikMutation<CreatePPPoEProfile, PPPoEProfileResponse>
{
    public string Command => "/ppp/profile/add";
    
    public Dictionary<string, string> BuildParameters(CreatePPPoEProfile request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["local-address"] = request.LocalAddress,
            ["remote-address"] = request.RemoteAddress,
            ["dns-server"] = request.DnsServers,
            ["rate-limit"] = request.RateLimit,
            ["only-one"] = request.OnlyOne,
        };
        
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoEProfileResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEProfileResponse { Id = rawResponse ?? string.Empty };
    }
}
