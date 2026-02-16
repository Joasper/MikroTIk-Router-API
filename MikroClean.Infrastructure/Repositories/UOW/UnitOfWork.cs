using MikroClean.Domain.Interfaces.UOW;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories.UOW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MikroCleanContext ctx;

        public UnitOfWork(MikroCleanContext ctx)
        {
            this.ctx = ctx;
        }
        public void Dispose()
        {
            ctx.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await ctx.SaveChangesAsync();
        }
    }
}
