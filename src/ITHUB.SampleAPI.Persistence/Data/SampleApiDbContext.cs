using ITHUB.SampleAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITHUB.SampleAPI.Persistence.Data;

public sealed class SampleApiDbContext(DbContextOptions<SampleApiDbContext> options) : DbContext(options)
{
    public DbSet<SampleUserEntity> Users => Set<SampleUserEntity>();
    public DbSet<ApplicationLogEntity> ApplicationLogs => Set<ApplicationLogEntity>();
    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();
    public DbSet<TaskActivityEntity> TaskActivities => Set<TaskActivityEntity>();
    public DbSet<StatusEntity> Statuses => Set<StatusEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleApiDbContext).Assembly);
    }
}
