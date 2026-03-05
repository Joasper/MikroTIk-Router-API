using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class SystemPermissionConfiguration : IEntityTypeConfiguration<SystemPermission>
    {
        public void Configure(EntityTypeBuilder<SystemPermission> builder)
        {
            builder.ToTable("SystemPermissions");
            
            builder.HasKey(sp => sp.Id);
            
            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(sp => sp.Description)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            // Relationships
            builder.HasMany(sp => sp.RolePermissions)
                .WithOne(srp => srp.Permission)
                .HasForeignKey(srp => srp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(sp => sp.Name)
                .IsUnique();
        }
    }
}
