using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;

/// <summary>
/// Operación para eliminar un PPPoE Secret del router
/// </summary>
public class DeletePppSecretOperation : IMikroTikMutation<DeletePPPoESecretRequest, PPPoESecretResponse>
{
    public string Command => "/ppp/secret/remove";
    
    public Dictionary<string, string> BuildParameters(DeletePPPoESecretRequest parameters)
    {
        return new Dictionary<string, string>
        {
            [".id"] = parameters.Id.Trim().ToLower()
        };
    }
    
    public PPPoESecretResponse ParseResponse(string? rawResponse)
    {
        return new PPPoESecretResponse();
    }
}
