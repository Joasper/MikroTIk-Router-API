using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.IpAddresses;

/// <summary>
/// Operación para agregar una dirección IP a una interfaz del router
/// </summary>
public class CreateIpAddressOperation : IMikroTikOperation<CreateIpAddressRequest, IpAddressResponse>
{
    public string Command => "/ip/address/add";

    public Dictionary<string, string> BuildParameters(CreateIpAddressRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            ["address"] = request.Address,
            ["interface"] = request.Interface,
            ["disabled"] = request.Disabled ? "yes" : "no"
        };

        if (!string.IsNullOrEmpty(request.Network))
        {
            parameters["network"] = request.Network;
        }

        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }

        return parameters;
    }

    public IpAddressResponse ParseResponse(ITikSentence response)
    {
        return new IpAddressResponse
        {
            Id = response.GetResponseField(".id"),
            Address = response.GetResponseField("address"),
            Network = response.GetResponseField("network"),
            Interface = response.GetResponseField("interface"),
            Invalid = response.GetResponseField("invalid") == "true",
            Disabled = response.GetResponseField("disabled") == "true",
            Dynamic = response.GetResponseField("dynamic") == "true"
        };
    }
}
