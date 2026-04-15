using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Firewall;

/// <summary>
/// Operación para crear una regla de firewall en el router MikroTik
/// </summary>
public class CreateFirewallRuleOperation : IMikroTikOperation<CreateFirewallRuleRequest, FirewallRuleResponse>
{
    public string Command => "/ip/firewall/filter/add";

    public Dictionary<string, string> BuildParameters(CreateFirewallRuleRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["chain"] = request.Chain,
            ["action"] = request.Action,
            ["disabled"] = request.Disabled ? "yes" : "no"
        };

        if (!string.IsNullOrEmpty(request.SrcAddress))
            parameters["src-address"] = request.SrcAddress;

        if (!string.IsNullOrEmpty(request.DstAddress))
            parameters["dst-address"] = request.DstAddress;

        if (!string.IsNullOrEmpty(request.Protocol))
            parameters["protocol"] = request.Protocol;

        if (!string.IsNullOrEmpty(request.DstPort))
            parameters["dst-port"] = request.DstPort;

        if (!string.IsNullOrEmpty(request.InInterface))
            parameters["in-interface"] = request.InInterface;

        if (!string.IsNullOrEmpty(request.OutInterface))
            parameters["out-interface"] = request.OutInterface;

        if (!string.IsNullOrEmpty(request.Comment))
            parameters["comment"] = request.Comment;

        return parameters;
    }

    public FirewallRuleResponse ParseResponse(ITikSentence response)
    {
        return new FirewallRuleResponse
        {
            Id = response.GetResponseField(".id"),
            Chain = response.GetResponseField("chain"),
            Action = response.GetResponseField("action"),
            SrcAddress = response.GetResponseField("src-address"),
            DstAddress = response.GetResponseField("dst-address"),
            Protocol = response.GetResponseField("protocol"),
            DstPort = response.GetResponseField("dst-port"),
            Bytes = long.TryParse(response.GetResponseField("bytes"), out var bytes) ? bytes : 0,
            Packets = long.TryParse(response.GetResponseField("packets"), out var packets) ? packets : 0,
            Disabled = response.GetResponseField("disabled") == "true",
            Invalid = response.GetResponseField("invalid") == "true",
            Comment = response.GetResponseField("comment")
        };
    }
}
