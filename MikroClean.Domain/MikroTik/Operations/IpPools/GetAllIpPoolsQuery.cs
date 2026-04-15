using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpPools;

/// <summary>
/// Consulta para obtener todos los IP Pools del router
/// </summary>
public class GetAllIpPoolsQuery : IMikroTikQuery<List<IpPoolResponse>>
{
    public string Command => "/ip/pool/print";

    public List<IpPoolResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var pools = new List<IpPoolResponse>();

        foreach (var sentence in responses)
        {
            pools.Add(new IpPoolResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                Ranges = sentence.GetResponseField("ranges"),
                NextPool = sentence.GetOptionalField("next-pool"),
                Comment = sentence.GetOptionalField("comment")
            });
        }

        return pools;
    }
}
