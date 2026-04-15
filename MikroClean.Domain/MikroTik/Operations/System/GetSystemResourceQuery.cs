using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.System;

/// <summary>
/// Consulta para obtener información de recursos del sistema del router
/// </summary>
public class GetSystemResourceQuery : IMikroTikQuery<SystemResourceResponse>
{
    public string Command => "/system/resource/print";

    public SystemResourceResponse ParseResponse(IEnumerable<ITikSentence> responses)
    {
        var sentence = responses.FirstOrDefault();
        if (sentence == null)
            throw new InvalidOperationException("No se recibió respuesta del router");

        return new SystemResourceResponse
        {
            Version = sentence.GetResponseField("version"),
            BoardName = sentence.GetResponseField("board-name"),
            Architecture = sentence.GetResponseField("architecture-name"),
            TotalMemory = long.TryParse(sentence.GetResponseField("total-memory"), out var totalMem) ? totalMem : 0,
            FreeMemory = long.TryParse(sentence.GetResponseField("free-memory"), out var freeMem) ? freeMem : 0,
            CpuLoad = double.TryParse(sentence.GetResponseField("cpu-load"), out var cpuLoad) ? cpuLoad : 0,
            TotalHddSpace = long.TryParse(sentence.GetResponseField("total-hdd-space"), out var totalHdd) ? totalHdd : 0,
            FreeHddSpace = long.TryParse(sentence.GetResponseField("free-hdd-space"), out var freeHdd) ? freeHdd : 0,
            Uptime = ParseUptime(sentence.GetResponseField("uptime"))
        };
    }

    /// <summary>
    /// Convierte el string de uptime (ej: "2w3d4h5m6s") a TimeSpan
    /// </summary>
    private TimeSpan ParseUptime(string uptime)
    {
        try
        {
            var totalSeconds = 0;
            var current = "";

            foreach (var c in uptime)
            {
                if (char.IsDigit(c))
                {
                    current += c;
                }
                else if (!string.IsNullOrEmpty(current))
                {
                    var value = int.Parse(current);
                    totalSeconds += c switch
                    {
                        'w' => value * 7 * 24 * 3600,
                        'd' => value * 24 * 3600,
                        'h' => value * 3600,
                        'm' => value * 60,
                        's' => value,
                        _ => 0
                    };
                    current = "";
                }
            }

            return TimeSpan.FromSeconds(totalSeconds);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}
