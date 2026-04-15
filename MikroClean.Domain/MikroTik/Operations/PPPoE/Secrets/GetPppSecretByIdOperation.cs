using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;

/// <summary>
/// Operación para obtener un PPPoE Secret específico por su ID
/// </summary>
public class GetPppSecretByIdOperation : IMikroTikOperation<string, PPPoESecretResponse?>
{
    public string Command => "/ppp/secret/print";
    
    public Dictionary<string, string> BuildParameters(string id)
    {
        return new Dictionary<string, string>
        {
            ["?.id"] = id
        };
    }
    
    public PPPoESecretResponse? ParseResponse(ITikSentence response)
    {
        if (response == null) return null;
        
        return new PPPoESecretResponse
        {
            Id = response.GetResponseField(".id"),
            Name = response.GetResponseField("name"),
            Profile = response.GetResponseField("profile"),
            Service = response.GetResponseField("service"),
            Disabled = response.GetResponseField("disabled") == "true" || response.GetResponseField("disabled") == "yes",
            Comment = response.GetOptionalField("comment")
        };
    }
}
