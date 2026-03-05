using MikroClean.Domain.Entities;
using MikroClean.Domain.Interfaces.Repositories;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class OrganizationRepository : BaseRepository<Organizations>, IOrganizationRepository
    {
        public OrganizationRepository(MikroCleanContext ctx) : base(ctx)
        {
        }
    }
}
