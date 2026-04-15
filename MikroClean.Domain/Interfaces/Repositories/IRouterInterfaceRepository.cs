using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories;

/// <summary>
/// Repositorio para gestionar interfaces de routers MikroTik
/// </summary>
public interface IRouterInterfaceRepository : IRepository<RouterInterface>
{
    /// <summary>
    /// Obtiene todas las interfaces de un router específico
    /// </summary>
    Task<List<RouterInterface>> GetByRouterIdAsync(int routerId);

    /// <summary>
    /// Busca una interfaz por su ID de MikroTik
    /// </summary>
    Task<RouterInterface?> GetByMikroTikIdAsync(int routerId, string mikroTikId);

    /// <summary>
    /// Busca una interfaz por nombre dentro de un router
    /// </summary>
    Task<RouterInterface?> GetByNameAsync(int routerId, string name);

    /// <summary>
    /// Obtiene interfaces de un router filtradas por tipo
    /// </summary>
    Task<List<RouterInterface>> GetByRouterIdAndTypeAsync(int routerId, string type);

    /// <summary>
    /// Obtiene interfaces que están running (activas)
    /// </summary>
    Task<List<RouterInterface>> GetRunningInterfacesAsync(int routerId);

    /// <summary>
    /// Obtiene interfaces que no están deshabilitadas
    /// </summary>
    Task<List<RouterInterface>> GetEnabledInterfacesAsync(int routerId);
}
