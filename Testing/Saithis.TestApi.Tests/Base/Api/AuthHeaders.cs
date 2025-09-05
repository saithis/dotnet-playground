using System.Net.Http.Headers;
using Saithis.Testing.Integration.XUnit.Api.Auth;

namespace Saithis.TestApi.Tests.Base.Api;

public static class AuthHeaders
{
    public static readonly AuthenticationHeaderValue NoScopes = new ApiTestAuthHeader(
        new ApiTestAuthHandler.AuthHeaderDto
        {
            UserId = null,
            Scopes = [],
            ClientId = "no.scopes",
        });
}
