using Saithis.Testing.Integration.XUnit.EFCore;

namespace Saithis.TestApi.Tests.Base.Api;

public record DbResetOptions
{
    public static readonly DbResetOptions Class = new()
    {
        Dummy = DbResetScope.Class,
    };

    public static readonly DbResetOptions None = new();

    public static readonly DbResetOptions Test = new()
    {
        Dummy = DbResetScope.Test,
    };

    public DbResetScope Dummy { get; init; } = DbResetScope.None;
}
