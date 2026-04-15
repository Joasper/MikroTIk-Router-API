using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;

/// <summary>
/// Operación para actualizar un servidor PPPoE existente en el router
/// </summary>
public class UpdatePppServerOperation : IMikroTikMutation<UpdatePPPoEServerRequest, PPPoEServerResponse>
{
    public string Command => "/interface/pppoe-server/server/set";
    
    public Dictionary<string, string> BuildParameters(UpdatePPPoEServerRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "numbers", request.Id.ToLower().Trim() }
        };
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            parameters["service-name"] = request.Name;
        }
        if (!string.IsNullOrEmpty(request.Interface))
        {
            parameters["interface"] = request.Interface;
        }
        if (!string.IsNullOrEmpty(request.Profile))
        {
            parameters["default-profile"] = request.Profile;
        }
        if (!string.IsNullOrEmpty(request.MaxMTU))
        {
            parameters["max-mtu"] = request.MaxMTU;
        }
        if (!string.IsNullOrEmpty(request.MaxMRU))
        {
            parameters["max-mru"] = request.MaxMRU;
        }
        if (!string.IsNullOrEmpty(request.KeepAliveTimeOut))
        {
            parameters["keepalive-timeout"] = request.KeepAliveTimeOut;
        }
        if (!string.IsNullOrEmpty(request.OneSesionPerHost))
        {
            parameters["one-session-per-host"] = request.OneSesionPerHost;
        }
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoEServerResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEServerResponse();
    }
}
