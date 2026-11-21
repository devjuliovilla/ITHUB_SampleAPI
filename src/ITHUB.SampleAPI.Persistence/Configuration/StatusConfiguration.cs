using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHUB.SampleAPI.Persistence.Configuration;

public sealed class StatusConfiguration : IEntityTypeConfiguration<StatusEntity>
{
    public void Configure(EntityTypeBuilder<StatusEntity> builder)
    {
        builder.ToTable("Statuses", "dbo");
        builder.HasKey(status => status.Id);
        builder.Property(status => status.Id).ValueGeneratedNever();
        builder.Property(status => status.Scope).HasMaxLength(20).IsRequired();
        builder.Property(status => status.Code).HasMaxLength(30).IsRequired();
        builder.Property(status => status.Name).HasMaxLength(100).IsRequired();
        builder.Property(status => status.IsActive).HasDefaultValue(true).IsRequired();
        builder.HasIndex(status => new { status.Scope, status.Code }).IsUnique();
    }
}
