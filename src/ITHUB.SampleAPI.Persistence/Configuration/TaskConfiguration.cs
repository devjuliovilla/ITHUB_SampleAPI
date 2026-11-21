using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ITHUB.SampleAPI.Persistence.Configuration;

public sealed class TaskConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks", "dbo");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).UseIdentityColumn();
        builder.Property(task => task.Title).HasMaxLength(200).IsRequired();
        builder.Property(task => task.Description).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(task => task.StatusId)
            .HasDefaultValue(StatusCatalogConstants.TaskAssignedId, "DF_Tasks_StatusId")
            .IsRequired();
        builder.Property(task => task.CreatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.Property(task => task.UpdatedAtUtc).HasColumnType("datetime2").IsRequired();
        builder.HasIndex(task => task.AssignedUserId);
        builder.HasIndex(task => task.StatusId);
        builder.HasOne(task => task.AssignedUser)
            .WithMany()
            .HasForeignKey(task => task.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(task => task.CreatedByUser)
            .WithMany()
            .HasForeignKey(task => task.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(task => task.Status)
            .WithMany()
            .HasForeignKey(task => task.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(task => task.Activities)
            .WithOne(activity => activity.Task)
            .HasForeignKey(activity => activity.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
