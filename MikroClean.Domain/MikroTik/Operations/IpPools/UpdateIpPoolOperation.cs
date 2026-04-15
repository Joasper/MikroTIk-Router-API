using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpPools;

/// <summary>
/// Operación para actualizar un IP Pool existente en el router
/// </summary>
public class UpdateIpPoolOperation : IMikroTikMutation<UpdateIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/set";

    public Dictionary<string, string> BuildParameters(UpdateIpPoolRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "numbers", request.Id.ToLower().Trim() }
        };

        if (!string.IsNullOrEmpty(request.Name))
        {
            parameters["name"] = request.Name;
        }

        if (!string.IsNullOrEmpty(request.Ranges))
        {
            parameters["ranges"] = request.Ranges;
        }

        if (!string.IsNullOrWhiteSpace(request.NextPool))
        {
            parameters["next-pool"] = request.NextPool;
        }

        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }

        return parameters;
    }

    /// <summary>
    /// La operación set no retorna datos útiles. El servicio debería hacer GetById después.
    /// </summary>
    public IpPoolResponse ParseResponse(string? rawResponse)
    {
        return new IpPoolResponse();
    }
}
