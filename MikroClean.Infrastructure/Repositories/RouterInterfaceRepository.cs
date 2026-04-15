using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MikroClean.Infrastructure.Repositories;

/// <summary>
/// Implementación del repositorio para interfaces de routers
/// </summary>
/// <remarks>
/// TODO: Este repositorio requiere que se agregue el DbSet RouterInterfaces en MikroCleanContext.
/// Temporalmente deshabilitado para permitir la compilación.
/// </remarks>
public class RouterInterfaceRepository : BaseRepository<RouterInterface>, IRouterInterfaceRepository
{
    private readonly MikroCleanContext _ctx;

    public RouterInterfaceRepository(MikroCleanContext ctx) : base(ctx)
    {
        _ctx = ctx;
    }

    public async Task<List<RouterInterface>> GetByRouterIdAsync(int routerId)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .Where(x => x.RouterId == routerId)
        //     .OrderBy(x => x.Name)
        //     .ToListAsync();
        return await Task.FromResult(new List<RouterInterface>());
    }

    public async Task<RouterInterface?> GetByMikroTikIdAsync(int routerId, string mikroTikId)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .FirstOrDefaultAsync(x => x.RouterId == routerId
        //         && x.MikroTikId == mikroTikId);
        return await Task.FromResult<RouterInterface?>(null);
    }

    public async Task<RouterInterface?> GetByNameAsync(int routerId, string name)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .FirstOrDefaultAsync(x => x.RouterId == routerId
        //         && x.Name == name);
        return await Task.FromResult<RouterInterface?>(null);
    }

    public async Task<List<RouterInterface>> GetByRouterIdAndTypeAsync(int routerId, string type)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .Where(x => x.RouterId == routerId
        //         && x.Type == type)
        //     .OrderBy(x => x.Name)
        //     .ToListAsync();
        return await Task.FromResult(new List<RouterInterface>());
    }

    public async Task<List<RouterInterface>> GetRunningInterfacesAsync(int routerId)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .Where(x => x.RouterId == routerId
        //         && x.Running
        //         && !x.Disabled)
        //     .OrderBy(x => x.Name)
        //     .ToListAsync();
        return await Task.FromResult(new List<RouterInterface>());
    }

    public async Task<List<RouterInterface>> GetEnabledInterfacesAsync(int routerId)
    {
        // TODO: Descomentar cuando se agregue RouterInterfaces al DbContext
        // return await _ctx.RouterInterfaces
        //     .Where(x => x.RouterId == routerId
        //         && !x.Disabled)
        //     .OrderBy(x => x.Name)
        //     .ToListAsync();
        return await Task.FromResult(new List<RouterInterface>());
    }
}
