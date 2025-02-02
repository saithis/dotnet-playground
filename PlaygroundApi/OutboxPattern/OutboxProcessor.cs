﻿using Medallion.Threading;
using Microsoft.EntityFrameworkCore;

namespace PlaygroundApi.OutboxPattern;

public class OutboxProcessor<TDbContext>(
    IServiceScopeFactory serviceScopeFactory, 
    IDistributedLockProvider distributedLockProvider, 
    TimeProvider timeProvider,
    ILogger<OutboxProcessor<TDbContext>> logger) 
    : BackgroundService where TDbContext : DbContext, IOutboxDbContext
{
    private readonly TimeSpan _dbCheckDelay = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _restartDelay = TimeSpan.FromSeconds(5);
    /// <remarks>
    /// This should not be longer than _dbCheckDelay, because if we cannot acquire a lock in that time,
    /// the next round of _dbCheckDelay on another node would pick up again anyway.
    /// </remarks>
    private readonly TimeSpan _lockAcquireTimeout = TimeSpan.FromSeconds(60);
    private const int BatchSize = 100;
    private CancellationTokenSource _cts = new();
    
    public Task ScheduleNowAsync()
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting OutboxProcessor");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxAsync(stoppingToken);
                await WaitTillDelayOverOrTriggeredAsync(stoppingToken);
            }
            catch (Exception e)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                logger.LogCritical(e, "OutboxProcessor crashed, trying to restart in {Delay}", _restartDelay);
                await Task.Delay(_restartDelay, timeProvider, stoppingToken);
                if (stoppingToken.IsCancellationRequested)
                    break;
            }
        }

        logger.LogInformation("Stopped OutboxProcessor");
    }

    private async Task ProcessOutboxAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Trying to acquire distributed lock");
        await using IDistributedSynchronizationHandle? dLock =
            await distributedLockProvider.TryAcquireLockAsync("OutboxProcessor", 
                _lockAcquireTimeout,
                stoppingToken);

        if (dLock == null)
        {
            logger.LogInformation("Failed to acquire lock, outbox processing will be skipped");
            return;
        }

        logger.LogDebug("Distributed lock acquired");
        
        try
        {
            using IServiceScope serviceScope = serviceScopeFactory.CreateScope();

            var dbContext = serviceScope.ServiceProvider.GetRequiredService<TDbContext>();
            var sender = serviceScope.ServiceProvider.GetRequiredService<IOutboxMessageSender>();
            while (true)
            {
                logger.LogDebug("Checking outbox for unsent messages");
                var messages = await dbContext.Set<OutboxMessageEntity>()
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(BatchSize)
                    .ToArrayAsync(stoppingToken);

                logger.LogInformation("Found {Count} messages to send", messages.Length);
                if (messages.Length == 0)
                    return;

                try
                {
                    foreach (var message in messages)
                    {
                        logger.LogInformation("Processing message '{Id}'", message.Id);
                        await sender.SendAsync(message, stoppingToken);
                        message.ProcessedAt = DateTime.UtcNow;
                    }
                }
                finally
                {
                    // We always want to save here, so that sent messages will not be sent again if possible
                    await dbContext.SaveChangesAsync(CancellationToken.None);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while processing outbox messages");
        }
    }

    private async Task WaitTillDelayOverOrTriggeredAsync(CancellationToken stoppingToken)
    {
        logger.LogDebug("Waiting for {Delay}", _dbCheckDelay);

        // If cts was already used, we need to prepare a new one here
        if (_cts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            ResetCts(stoppingToken);
        }

        await TaskHelper.DelayWithoutExceptionAsync(_dbCheckDelay, timeProvider, _cts.Token);

        if (_cts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            logger.LogDebug("Delay was cancelled");
        }
    }

    private void ResetCts(CancellationToken stoppingToken)
    {
        _cts.Dispose();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
    }

    private static class TaskHelper
    {
        public static async Task DelayWithoutExceptionAsync(TimeSpan delay, TimeProvider timeProvider, CancellationToken ct)
        {
            // https://blog.stephencleary.com/2023/11/configureawait-in-net-8.html
            await Task.Delay(delay, timeProvider, ct)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
        }
    }
}