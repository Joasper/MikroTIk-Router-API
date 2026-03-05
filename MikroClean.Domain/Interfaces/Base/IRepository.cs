using MikroClean.Domain.Entities.Base;
using System.Linq.Expressions;

namespace MikroClean.Domain.Interfaces.Base
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity?> GetByExpressionAsync(Expression<Func<TEntity, bool>> expression);
        void Add(TEntity entity);
        void UpdateAsync(TEntity entity);
        void DeleteAsync(TEntity entity);
    }
}
