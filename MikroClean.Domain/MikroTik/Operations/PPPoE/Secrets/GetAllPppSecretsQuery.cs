using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.PPPoE.Secrets;

/// <summary>
/// Consulta para obtener todos los PPPoE Secrets del router
/// </summary>
public class GetAllPppSecretsQuery : IMikroTikQuery<List<PPPoESecretResponse>>
{
    public string Command => "/ppp/secret/print";
    
    public List<PPPoESecretResponse> ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var secrets = new List<PPPoESecretResponse>();
        
        foreach (var sentence in responses)
        {
            secrets.Add(new PPPoESecretResponse
            {
                Id = sentence.GetResponseField(".id"),
                Name = sentence.GetResponseField("name"),
                Profile = sentence.GetResponseField("profile"),
                Service = sentence.GetResponseField("service"),
                Disabled = sentence.GetResponseField("disabled") == "true" || sentence.GetResponseField("disabled") == "yes",
                Comment = sentence.GetOptionalField("comment")
            });
        }
        
        return secrets;
    }
}
