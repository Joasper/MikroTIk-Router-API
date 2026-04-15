using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;

/// <summary>
/// Operación para crear un nuevo PPPoE Secret en el router
/// </summary>
public class CreatePppSecretOperation : IMikroTikMutation<CreatePPPoESecretRequest, PPPoESecretResponse>
{
    public string Command => "/ppp/secret/add";
    
    public Dictionary<string, string> BuildParameters(CreatePPPoESecretRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["password"] = request.Password,
            ["profile"] = request.Profile,
            ["service"] = request.Service,
            ["disabled"] = request.Disabled ? "yes" : "no",
        };
        
        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }
        
        return parameters;
    }
    
    public PPPoESecretResponse ParseResponse(string? rawResponse)
    {
        return new PPPoESecretResponse { Id = rawResponse ?? string.Empty };
    }
}
