namespace PlaygroundApi.OutboxPattern;

public static class OutboxServiceCollectionExtensions
{
    public static IServiceCollection AddOutboxPattern<TDbContext>(this IServiceCollection services)
        where TDbContext : IOutboxDbContext
    {
        services.AddSingleton<OutboxProcessor<TDbContext>>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<OutboxProcessor<TDbContext>>());
        return services;
    }
}