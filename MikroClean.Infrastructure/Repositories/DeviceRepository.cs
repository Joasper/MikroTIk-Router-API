using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class DeviceRepository : BaseRepository<Device>, IDeviceRepository
    {
        public DeviceRepository(MikroCleanContext ctx) : base(ctx)
        {
        }
    }
}
