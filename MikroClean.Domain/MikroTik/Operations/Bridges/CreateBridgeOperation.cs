using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Bridges;

/// <summary>
/// Operación para crear una interfaz Bridge en el router MikroTik
/// </summary>
public class CreateBridgeOperation : IMikroTikOperation<CreateBridgeRequest, BridgeResponse>
{
    public string Command => "/interface/bridge/add";

    public Dictionary<string, string> BuildParameters(CreateBridgeRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["name"] = request.Name,
            ["admin-mac"] = request.AdminMac ? "yes" : "no",
            ["ageing-time"] = request.AgingTime.ToString(),
            ["disabled"] = request.Disabled ? "yes" : "no"
        };

        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }

        return parameters;
    }

    public BridgeResponse ParseResponse(ITikSentence response)
    {
        return new BridgeResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            MacAddress = response.GetResponseField("mac-address"),
            Running = response.GetResponseField("running") == "true",
            Disabled = response.GetResponseField("disabled") == "true",
            Comment = response.GetResponseField("comment")
        };
    }
}
