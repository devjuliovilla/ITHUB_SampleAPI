using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHUB.SampleAPI.Persistence.Configuration;

public sealed class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLogEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationLogEntity> builder)
    {
        builder.ToTable("ApplicationLogs", "dbo");
        builder.HasKey(log => log.Id);
        builder.Property(log => log.Id).UseIdentityColumn();
        builder.Property(log => log.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(log => log.Level).HasMaxLength(20).IsRequired();
        builder.Property(log => log.EventName).HasMaxLength(100).IsRequired();
        builder.Property(log => log.Message).HasMaxLength(4000).IsRequired();
        builder.Property(log => log.PropertiesJson).HasColumnType("nvarchar(max)");
        builder.Property(log => log.TraceId).HasMaxLength(100);
        builder.Property(log => log.UserEmail).HasMaxLength(320);
        builder.HasIndex(log => log.CreatedAtUtc);
        builder.HasIndex(log => log.EventName);
    }
}
