using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Vlans;

/// <summary>
/// Operación para crear una interfaz VLAN en el router MikroTik
/// </summary>
public class CreateVlanOperation : IMikroTikOperation<CreateVlanRequest, VlanResponse>
{
    public string Command => "/interface/vlan/add";

    public Dictionary<string, string> BuildParameters(CreateVlanRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["vlan-id"] = request.VlanId.ToString(),
            ["interface"] = request.Interface,
            ["disabled"] = request.Disabled ? "yes" : "no"
        };

        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }

        return parameters;
    }

    public VlanResponse ParseResponse(ITikSentence response)
    {
        return new VlanResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            VlanId = int.Parse(response.GetResponseField("vlan-id")),
            Interface = response.GetResponseField("interface"),
            Running = response.GetResponseField("running") == "true",
            Disabled = response.GetResponseField("disabled") == "true",
            Comment = response.GetResponseField("comment")
        };
    }
}
