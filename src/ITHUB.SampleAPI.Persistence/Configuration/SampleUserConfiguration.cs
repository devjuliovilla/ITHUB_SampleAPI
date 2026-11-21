using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHUB.SampleAPI.Persistence.Configuration;

public sealed class SampleUserConfiguration : IEntityTypeConfiguration<SampleUserEntity>
{
    public void Configure(EntityTypeBuilder<SampleUserEntity> builder)
    {
        builder.ToTable("Users", "dbo");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).UseIdentityColumn();
        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(user => user.Role).HasMaxLength(20).HasDefaultValue("User").IsRequired();
        builder.Property(user => user.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}
