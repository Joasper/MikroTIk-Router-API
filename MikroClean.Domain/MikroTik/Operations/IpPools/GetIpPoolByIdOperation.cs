using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpPools;

/// <summary>
/// Operación para obtener un IP Pool específico por su ID
/// </summary>
public class GetIpPoolByIdOperation : IMikroTikOperation<string, IpPoolResponse?>
{
    public string Command => "/ip/pool/print";

    public Dictionary<string, string> BuildParameters(string id)
    {
        return new Dictionary<string, string>
        {
            ["?.id"] = id
        };
    }

    public IpPoolResponse? ParseResponse(ITikSentence response)
    {
        if (response == null) return null;

        return new IpPoolResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            Ranges = response.GetResponseField("ranges"),
            NextPool = response.GetOptionalField("next-pool"),
            Comment = response.GetOptionalField("comment")
        };
    }
}
