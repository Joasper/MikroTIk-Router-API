using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpPools;

/// <summary>
/// Operación para eliminar un IP Pool del router
/// </summary>
public class DeleteIpPoolOperation : IMikroTikMutation<DeleteIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/remove";

    public Dictionary<string, string> BuildParameters(DeleteIpPoolRequest request)
    {
        return new Dictionary<string, string>
        {
            [".id"] = request.Id
        };
    }

    /// <summary>
    /// La operación remove no retorna datos útiles
    /// </summary>
    public IpPoolResponse ParseResponse(string? rawResponse)
    {
        return new IpPoolResponse();
    }
}
