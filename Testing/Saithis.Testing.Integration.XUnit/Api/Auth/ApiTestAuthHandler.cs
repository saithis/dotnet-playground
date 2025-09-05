
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Saithis.Testing.Integration.XUnit.Api.Auth;

public class ApiTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder), IAuthenticationRequestHandler
{
    public const string SchemeName = "Dto";

    public Task<bool> HandleRequestAsync() => Task.FromResult(false);

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetDtoAuthHeader(out string authHeaderData))
            return Task.FromResult(AuthenticateResult.NoResult());

        AuthenticationTicket ticket = CreateAuthenticationTicket(authHeaderData);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetDtoAuthHeader(out string authHeaderData)
    {
        const string headerStart = $"Bearer {SchemeName} ";
        authHeaderData = Request.Headers.Authorization.ToString();
        if (!authHeaderData.StartsWith(headerStart, StringComparison.OrdinalIgnoreCase))
            return false;
        authHeaderData = authHeaderData[headerStart.Length..];
        return true;
    }

    private static AuthenticationTicket CreateAuthenticationTicket(string authHeaderData)
    {
        var data = JsonSerializer.Deserialize<AuthHeaderDto>(authHeaderData);
        if (data == null)
            throw new InvalidOperationException($"Malformed auth header: {authHeaderData}");

        var claims = new List<Claim>
        {
            new("iss", "https://localhost/"),
        };
        claims.AddRange(data.Scopes.Select(scope => new Claim("scope", scope)));
        if (data.ClientId != null)
        {
            claims.Add(new Claim("client_id", data.ClientId));
        }

        if (data.UserId != null)
        {
            claims.Add(new Claim("sub", data.UserId.ToString()!));
        }

        if (data.AdditionalClaims != null)
        {
            foreach (var (key, value) in data.AdditionalClaims)
            {
                claims.Add(new Claim(key, value));
            }
        }

        var identity = new ClaimsIdentity(claims, "Test", "sub", "role");
        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, SchemeName);
        return ticket;
    }

    public record AuthHeaderDto
    {
        public string[] Scopes { get; init; } = [];
        public string? ClientId { get; init; }
        public Guid? UserId { get; init; }
        public Dictionary<string, string>? AdditionalClaims { get; init; }
    }
}
