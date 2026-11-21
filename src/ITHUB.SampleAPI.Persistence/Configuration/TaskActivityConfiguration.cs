using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHUB.SampleAPI.Persistence.Configuration;

public sealed class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivityEntity>
{
    public void Configure(EntityTypeBuilder<TaskActivityEntity> builder)
    {
        builder.ToTable("TaskActivities", "dbo");
        builder.HasKey(activity => activity.Id);
        builder.Property(activity => activity.Id).UseIdentityColumn();
        builder.Property(activity => activity.Title).HasMaxLength(200).IsRequired();
        builder.Property(activity => activity.Description).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(activity => activity.StatusId)
            .HasDefaultValue(StatusCatalogConstants.ActivityPendingId, "DF_TaskActivities_StatusId")
            .IsRequired();
        builder.Property(activity => activity.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(activity => activity.UpdatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(activity => activity.TaskId);
        builder.HasIndex(activity => activity.StatusId);
        builder.HasOne(activity => activity.CreatedByUser)
            .WithMany()
            .HasForeignKey(activity => activity.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(activity => activity.Status)
            .WithMany()
            .HasForeignKey(activity => activity.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
