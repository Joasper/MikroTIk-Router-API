using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Profiles;

/// <summary>
/// Operación para eliminar un perfil PPPoE del router
/// </summary>
public class DeletePppProfileOperation : IMikroTikMutation<DeletePPPoEProfile, PPPoEProfileResponse>
{
    public string Command => "/ppp/profile/remove";
    
    public Dictionary<string, string> BuildParameters(DeletePPPoEProfile parameters)
    {
        return new Dictionary<string, string>
        {
            [".id"] = parameters.Id.Trim().ToLower()
        };
    }
    
    public PPPoEProfileResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEProfileResponse();
    }
}
