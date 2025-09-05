
using Microsoft.AspNetCore.Authentication;

namespace Saithis.Testing.Integration.XUnit.Api.Auth;

public static class AuthenticationBuilderExtensions
{
    /// <summary>
    /// Adds a mock scheme for testing purposes.
    /// See <see cref="ApiTestAuthHandler"/> and <see cref="ApiTestAuthHeader"/> for more.
    /// </summary>
    public static AuthenticationBuilder AddApiTestScheme(
        this AuthenticationBuilder builder,
        string authenticationScheme = ApiTestAuthHandler.SchemeName,
        Action<AuthenticationSchemeOptions>? configureOptions = null)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, ApiTestAuthHandler>(
            authenticationScheme,
            configureOptions ?? (_ => { }));
    }
}
