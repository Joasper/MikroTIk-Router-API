using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;

/// <summary>
/// Operación para crear un nuevo servidor PPPoE en el router
/// </summary>
public class CreatePppServerOperation : IMikroTikMutation<CreatePPPoEServerRequest, PPPoEServerResponse>
{
    public string Command => "/interface/pppoe-server/server/add";
    
    public Dictionary<string, string> BuildParameters(CreatePPPoEServerRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["service-name"] = request.Name,
            ["interface"] = request.Interface,
            ["default-profile"] = request.Profile,
            ["max-mtu"] = request.MaxMTU,
            ["max-mru"] = request.MaxMRU,
            ["keepalive-timeout"] = request.KeepAliveTimeOut,
            ["one-session-per-host"] = request.OneSesionPerHost,
            ["disabled"] = "no"
        };
        
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoEServerResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEServerResponse { Id = rawResponse ?? string.Empty };
    }
}
