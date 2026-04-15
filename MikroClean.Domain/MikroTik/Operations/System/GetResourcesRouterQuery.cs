using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.System;

/// <summary>
/// Consulta para obtener información básica del router (versión, recursos)
/// </summary>
public class GetResourcesRouterQuery : IMikroTikQuery<ResourcesRouterResponse>
{
    public string Command => "/system/resource/print";
    
    public ResourcesRouterResponse ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var sentence = responses.FirstOrDefault();
        if (sentence == null) return new ResourcesRouterResponse();
        
        return new ResourcesRouterResponse
        {
            Version = sentence.GetResponseField("version"),
            BoardName = sentence.GetResponseField("board-name"),
            Architecture = sentence.GetResponseField("architecture-name"),
            TotalMemory = long.TryParse(sentence.GetResponseField("total-memory"), out var totalMemory) ? totalMemory : 0,
            FreeMemory = long.TryParse(sentence.GetResponseField("free-memory"), out var freeMemory) ? freeMemory : 0,
            CpuLoad = double.TryParse(sentence.GetResponseField("cpu-load"), out var cpuLoad) ? cpuLoad : 0,
            TotalHddSpace = long.TryParse(sentence.GetResponseField("total-hdd-space"), out var totalHddSpace) ? totalHddSpace : 0,
            FreeHddSpace = long.TryParse(sentence.GetResponseField("free-hdd-space"), out var freeHddSpace) ? freeHddSpace : 0,
            Uptime = sentence.GetResponseField("uptime")
        };
    }
}
