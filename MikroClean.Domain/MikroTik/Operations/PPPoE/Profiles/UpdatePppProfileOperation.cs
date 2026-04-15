using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;

/// <summary>
/// Operación para actualizar un perfil PPPoE existente en el router
/// </summary>
public class UpdatePppProfileOperation : IMikroTikMutation<UpdatePPPoEProfile, PPPoEProfileResponse>
{
    public string Command => "/ppp/profile/set";
    
    public Dictionary<string, string> BuildParameters(UpdatePPPoEProfile request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "numbers", request.Id.ToLower().Trim() }
        };
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            parameters["name"] = request.Name;
        }
        if (!string.IsNullOrEmpty(request.LocalAddress))
        {
            parameters["local-address"] = request.LocalAddress;
        }
        if (!string.IsNullOrEmpty(request.RemoteAddress))
        {
            parameters["remote-address"] = request.RemoteAddress;
        }
        if (!string.IsNullOrEmpty(request.DnsServers))
        {
            parameters["dns-server"] = request.DnsServers;
        }
        if (!string.IsNullOrEmpty(request.RateLimit))
        {
            parameters["rate-limit"] = request.RateLimit;
        }
        if (!string.IsNullOrEmpty(request.OnlyOne))
        {
            parameters["only-one"] = request.OnlyOne;
        }
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoEProfileResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEProfileResponse();
    }
}
