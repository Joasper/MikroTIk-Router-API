using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Domain.MikroTik.Operations.Interfaces;

/// <summary>
/// Operación para actualizar una interfaz existente en el router MikroTik
/// </summary>
public class UpdateInterfaceOperation : IMikroTikMutation<UpdateInterfaceRequest, InterfaceUpdateResponse>
{
    public string Command => "/interface/set";

    public Dictionary<string, string> BuildParameters(UpdateInterfaceRequest request)
    {
        var parameters = new Dictionary<string, string>
        {
            { "numbers", request.MikroTikId.ToLower().Trim() }
        };

        if (!string.IsNullOrEmpty(request.Name))
        {
            parameters["name"] = request.Name;
        }

        if (request.Mtu.HasValue)
        {
            parameters["mtu"] = request.Mtu.Value.ToString();
        }

        if (request.Disabled.HasValue)
        {
            parameters["disabled"] = request.Disabled.Value ? "yes" : "no";
        }

        if (!string.IsNullOrEmpty(request.Comment))
        {
            parameters["comment"] = request.Comment;
        }

        return parameters;
    }

    /// <summary>
    /// La operación set no retorna datos útiles. El servicio debería hacer GetById después.
    /// </summary>
    public InterfaceUpdateResponse ParseResponse(string? rawResponse)
    {
        return new InterfaceUpdateResponse();
    }
}

/// <summary>
/// Request para actualizar una interfaz
/// </summary>
public class UpdateInterfaceRequest
{
    public string MikroTikId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int? Mtu { get; set; }
    public bool? Disabled { get; set; }
    public string? Comment { get; set; }
}

/// <summary>
/// Respuesta de actualización de interfaz
/// </summary>
public class InterfaceUpdateResponse
{
    // Vacío - el servicio hará GetById para obtener los datos actualizados
}
