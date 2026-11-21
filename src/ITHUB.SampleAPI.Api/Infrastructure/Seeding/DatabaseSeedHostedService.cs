using Microsoft.Extensions.DependencyInjection;

namespace ITHUB.SampleAPI.Api.Infrastructure.Seeding;

public sealed class DatabaseSeedHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var seedData = scope.ServiceProvider.GetRequiredService<SampleApiSeedData>();
        await seedData.SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
