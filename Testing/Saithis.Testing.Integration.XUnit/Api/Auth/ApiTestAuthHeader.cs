
using System.Net.Http.Headers;
using System.Text.Json;

namespace Saithis.Testing.Integration.XUnit.Api.Auth;

/// <summary>
/// Will be used to create an authentication header for API tests.<br />
/// format will be:<br />
/// Authentication: Bearer Dto <i>{JSON serialized <see cref="ApiTestAuthHandler.AuthHeaderDto"/>}</i>
/// </summary>
/// <param name="data"></param>
public class ApiTestAuthHeader(ApiTestAuthHandler.AuthHeaderDto data)
    : AuthenticationHeaderValue("Bearer", $"{ApiTestAuthHandler.SchemeName} {JsonSerializer.Serialize(data)}")
{
}
