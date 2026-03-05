using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class SystemRoleConfiguration : IEntityTypeConfiguration<SystemRole>
    {
        public void Configure(EntityTypeBuilder<SystemRole> builder)
        {
            builder.ToTable("SystemRoles");
            
            builder.HasKey(sr => sr.Id);
            
            builder.Property(sr => sr.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            // Relationships
            builder.HasMany(sr => sr.RolePermissions)
                .WithOne(srp => srp.Role)
                .HasForeignKey(srp => srp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(sr => sr.Name)
                .IsUnique();
        }
    }
}
