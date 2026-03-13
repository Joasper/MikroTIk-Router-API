using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Base;

namespace MikroClean.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio específico para routers con métodos especializados
    /// </summary>
    public interface IRouterRepository : IRepository<Router>
    {
        /// <summary>
        /// Obtiene todos los routers activos de una organización
        /// </summary>
        Task<IEnumerable<Router>> GetByOrganizationIdAsync(int organizationId);

        /// <summary>
        /// Obtiene un router por IP
        /// </summary>
        Task<Router?> GetByIpAsync(string ip);

        /// <summary>
        /// Obtiene routers disponibles (IsActive = true y no eliminados)
        /// </summary>
        Task<IEnumerable<Router>> GetAvailableRoutersAsync(int organizationId);

        /// <summary>
        /// Actualiza el LastSeen de un router
        /// </summary>
        Task UpdateLastSeenAsync(int routerId, DateTime lastSeen);
    }
}
