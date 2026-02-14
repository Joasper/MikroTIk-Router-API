namespace MikroClean.Domain.Interfaces.UOW
{
    public interface IUnitOfWork : IDisposable
    {
       Task<int> SaveChangesAsync();
    }
}
