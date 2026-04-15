using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE;

/// <summary>
/// Operación para eliminar una conexión PPPoE activa del router
/// </summary>
public class DeletePPPoEActiveConnectionOperation : IMikroTikMutation<DeletePPPoEActiveConnectionRequest, PPPoEActiveConnectionResponse>
{
    public string Command => "/ppp/active/remove";

    public Dictionary<string, string> BuildParameters(DeletePPPoEActiveConnectionRequest request)
    {
        return new Dictionary<string, string>
        {
            [".id"] = request.Id.Trim().ToLower()
        };
    }

    public PPPoEActiveConnectionResponse ParseResponse(string? rawResponse)
    {
        return new PPPoEActiveConnectionResponse
        {
            Id = rawResponse ?? string.Empty
        };
    }
}
