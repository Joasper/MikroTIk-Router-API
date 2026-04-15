using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpPools;

/// <summary>
/// Operación para crear un nuevo IP Pool en el router
/// </summary>
public class CreateIpPoolOperation : IMikroTikMutation<CreateIpPoolRequest, IpPoolResponse>
{
    public string Command => "/ip/pool/add";
    
    public Dictionary<string, string> BuildParameters(CreateIpPoolRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["ranges"] = request.Ranges,
        };

        if (!string.IsNullOrEmpty(request.NextPool))
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
    /// Parsea la respuesta de la creación. MikroTik retorna el ID directamente ("*9")
    /// </summary>
    public IpPoolResponse ParseResponse(string? rawResponse)
    {
        return new IpPoolResponse { Id = rawResponse ?? string.Empty };
    }
}
