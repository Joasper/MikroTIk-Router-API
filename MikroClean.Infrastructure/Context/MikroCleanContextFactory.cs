using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MikroClean.Infrastructure.Context
{
    public class MikroCleanContextFactory : IDesignTimeDbContextFactory<MikroCleanContext>
    {
        public MikroCleanContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MikroCleanContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=MikroCleanDB;User Id=sa;Password=TuPasswordFuerte123!;TrustServerCertificate=True;");
            return new MikroCleanContext(optionsBuilder.Options);
        }
    }
}
