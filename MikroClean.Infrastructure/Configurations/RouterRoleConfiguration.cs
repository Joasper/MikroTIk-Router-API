using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikroClean.Domain.Entities;

namespace MikroClean.Infrastructure.Configurations
{
    public class RouterRoleConfiguration : IEntityTypeConfiguration<RouterRole>
    {
        public void Configure(EntityTypeBuilder<RouterRole> builder)
        {
            builder.ToTable("RouterRoles");
            
            builder.HasKey(rr => rr.Id);
            
            builder.Property(rr => rr.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
            
            builder.Property(rr => rr.Description)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            // Relationships
            builder.HasMany(rr => rr.RolePermissions)
                .WithOne(rrp => rrp.Role)
                .HasForeignKey(rrp => rrp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(rr => rr.UserRouterAccesses)
                .WithOne(ura => ura.Role)
                .HasForeignKey(ura => ura.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(rr => rr.Name)
                .IsUnique();
        }
    }
}
