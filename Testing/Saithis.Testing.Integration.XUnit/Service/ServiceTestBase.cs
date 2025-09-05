
using Microsoft.EntityFrameworkCore;
using Saithis.Testing.Integration.XUnit.EFCore;

namespace Saithis.Testing.Integration.XUnit.Service;

/// <summary>
/// You have to inherit from this class to create a base class for your test classes.
/// This handles setup and cleanup calls to your fisture and provides a way to create a scope context for your tests.
/// </summary>
public abstract class ServiceTestBase<TFixture, TScope>(TFixture fixture)
    where TFixture : IServiceFxiture
    where TScope : ServiceScopeContext
{
    private static readonly HashSet<string> Initialized = [];
    private readonly List<Func<Task>> _initOnceTasks = [];
    private readonly List<Func<Task>> _initPerTestTasks = [];

    protected TFixture Fixture { get; init; } = fixture;

    [Before(Test)]
    public async Task InitializeTestAsync()
    {
        IsFirstClassInit = Initialized.Add(GetType().FullName ?? GetType().Name);

        await Fixture.SetupTestContextAsync();

        foreach (Func<Task> task in _initPerTestTasks)
        {
            await task();
        }

        await InitializePerTestAsync();
        if (IsFirstClassInit)
        {
            foreach (Func<Task> task in _initOnceTasks)
            {
                await task();
            }

            await InitializeOnceAsync();
        }
    }

    protected bool IsFirstClassInit { get; private set; }

    public async Task DisposeAsync()
    {
        await Fixture.CleanupTestContextAsync();
    }

    protected virtual Task InitializeOnceAsync() => Task.CompletedTask;
    protected virtual Task InitializePerTestAsync() => Task.CompletedTask;

    protected void RegisterInitOnceTask(Func<Task> task)
    {
        _initOnceTasks.Add(task);
    }

    protected void RegisterInitPerTestTask(Func<Task> task)
    {
        _initPerTestTasks.Add(task);
    }


    protected abstract TScope CreateScopeContext();

    protected async Task InScopeAsync(Func<TScope, Task> arrange)
    {
        using TScope ctx = CreateScopeContext();
        await arrange(ctx);
    }

    protected async Task InScopeAsync(Action<TScope> arrange)
    {
        await InScopeAsync(ctx =>
        {
            arrange(ctx);
            return Task.CompletedTask;
        });
    }

    protected async Task<TRes> InScopeAsync<TRes>(Func<TScope, Task<TRes>> arrange)
    {
        using TScope ctx = CreateScopeContext();
        TRes result = await arrange(ctx);
        return result;
    }

    protected async Task<TRes> InScopeAsync<TRes>(Func<TScope, TRes> arrange)
    {
        return await InScopeAsync(ctx =>
        {
            TRes result = arrange(ctx);
            return Task.FromResult(result);
        });
    }

    protected void RegisterDbResetMode<T>(IDbContextManager<T> dbContextManager, DbResetScope mode)
        where T : DbContext
    {
        _initPerTestTasks.Add(() =>
        {
            if (mode == DbResetScope.None || !IsFirstClassInit && mode == DbResetScope.Class)
            {
                return Task.CompletedTask;
            }

            dbContextManager.DeleteDatabase();
            return Task.CompletedTask;
        });
    }
}
