using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Servers;

/// <summary>
/// Operación para eliminar un servidor PPPoE del router
/// </summary>
public class DeletePppServerOperation : IMikroTikMutation<DeletePPPoEServerRequest, PPPoEServerResponse>
{
    public string Command => "/interface/pppoe-server/server/remove";
    
    public Dictionary<string, string> BuildParameters(DeletePPPoEServerRequest parameters)
    {
        return new Dictionary<string, string>
        {
            [".id"] = parameters.Id.Trim().ToLower()
        };
    }
    
    public PPPoEServerResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEServerResponse();
    }
}
