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
            throw new NotImplementedException();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await ctx.SaveChangesAsync();
        }
    }
}
