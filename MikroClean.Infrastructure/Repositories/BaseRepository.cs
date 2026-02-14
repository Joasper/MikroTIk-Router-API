using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Interfaces.Base;
using MikroClean.Infrastructure.Context;

namespace MikroClean.Infrastructure.Repositories
{
    public class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly MikroCleanContext ctx;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(MikroCleanContext ctx)
        {
            this.ctx = ctx;
        }
        public async Task AddAsync(T entity)
        {
            await ctx.Set<T>().AddAsync(entity);
        }

        public void DeleteAsync(T Entity)
        {
            ctx.Set<T>().Remove(Entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await ctx.Set<T>().ToListAsync();

        public Task<T?> GetByIdAsync(int id)
        {
            var entity = ctx.Set<T>().FirstOrDefault(t => t.Id == id);
            return Task.FromResult(entity);
        }

        public void UpdateAsync(T entity)
        {
              ctx.Set<T>().Update(entity);
        }
    }
}
