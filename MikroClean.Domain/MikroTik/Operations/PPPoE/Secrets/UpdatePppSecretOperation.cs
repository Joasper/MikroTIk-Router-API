using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;

/// <summary>
/// Operación para actualizar un PPPoE Secret existente en el router
/// </summary>
public class UpdatePppSecretOperation : IMikroTikMutation<UpdatePPPoESecretRequest, PPPoESecretResponse>
{
    public string Command => "/ppp/secret/set";
    
    public Dictionary<string, string> BuildParameters(UpdatePPPoESecretRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "numbers", request.Id.ToLower().Trim() }
        };
        
        if (!string.IsNullOrEmpty(request.Name))
        {
            parameters["name"] = request.Name;
        }
        if (!string.IsNullOrEmpty(request.Password))
        {
            parameters["password"] = request.Password;
        }
        if (!string.IsNullOrEmpty(request.Profile))
        {
            parameters["profile"] = request.Profile;
        }
        if (!string.IsNullOrEmpty(request.Service))
        {
            parameters["service"] = request.Service;
        }
        if (request.Disabled.HasValue)
        {
            parameters["disabled"] = request.Disabled.Value ? "yes" : "no";
        }
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoESecretResponse ParseResponse(string? rawResponse)
    {
        return new PPPoESecretResponse();
    }
}
