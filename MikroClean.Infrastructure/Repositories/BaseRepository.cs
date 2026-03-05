using Microsoft.EntityFrameworkCore;
using MikroClean.Domain.Entities.Base;
using MikroClean.Domain.Interfaces.Base;
using MikroClean.Infrastructure.Context;
using System.Linq.Expressions;

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
        public async void Add(T entity)
        {
            await ctx.Set<T>().AddAsync(entity);
        }

        public void DeleteAsync(T Entity)
        {
            ctx.Set<T>().Remove(Entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await ctx.Set<T>().ToListAsync();

        public async Task<T?> GetByExpressionAsync(Expression<Func<T, bool>> expression)
        {
            return await ctx.Set<T>().FirstOrDefaultAsync(expression);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await ctx.Set<T>().FirstOrDefaultAsync(t => t.Id == id);
        }

        public void UpdateAsync(T entity)
        {
              ctx.Set<T>().Update(entity);
        }
    }
}
