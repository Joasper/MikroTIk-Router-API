using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class UserRouterAccessConfiguration : IEntityTypeConfiguration<UserRouterAccess>
    {
        public void Configure(EntityTypeBuilder<UserRouterAccess> builder)
        {
            builder.ToTable("UserRouterAccess");
            
            builder.HasKey(ura => new { ura.UserId, ura.RouterId });
            
            builder.Property(ura => ura.UserId)
                .IsRequired();
            
            builder.Property(ura => ura.RouterId)
                .IsRequired();
            
            builder.Property(ura => ura.RoleId)
                .IsRequired();

            // Indexes
            builder.HasIndex(ura => ura.RouterId);
            builder.HasIndex(ura => ura.RoleId);
        }
    }
}
