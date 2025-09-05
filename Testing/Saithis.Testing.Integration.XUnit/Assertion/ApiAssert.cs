
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Sdk;

namespace Saithis.Testing.Integration.XUnit.Assertion;

public static class ApiAssert
{
    public static void ProblemDetailsResult(IResult result, int statusCode, string code)
    {
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(statusCode, problem.StatusCode);
        var baseProblem = Assert.IsType<ProblemDetails>(problem.ProblemDetails);
        AssertProblemDetails(baseProblem, statusCode, code);
    }

    public static T OkResult<T>(IResult result)
    {
        var okResult = Assert.IsType<Ok<T>>(result);
        var dto = Assert.IsType<T>(okResult.Value);
        return dto;
    }

    public static async Task ResponseAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {expected}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new XunitException(message);
        }
    }

    public static async Task<T?> ResponseAsync<T>(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ResponseAsync(response, expected);

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public static async Task<string> ResponseStringAsync(HttpResponseMessage response, HttpStatusCode expected)
    {
        await ResponseAsync(response, expected);

        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<ProblemDetails> ProblemResponseAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string type)
    {
        if (response.StatusCode != statusCode)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {statusCode}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new XunitException(message);
        }

        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problemDetails);
            AssertProblemDetails(problemDetails, (int) statusCode, type);
            return problemDetails;
        }
        catch (Exception e)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"ProblemResponse() Failure: Values differ{Environment.NewLine}" +
                             $"Expected ProblemDetails{Environment.NewLine}Actual Body:{body}";
            throw new XunitException(message, e);
        }
    }

    public static async Task<HttpValidationProblemDetails> IsValidationProblemResponseAsync(
        HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.BadRequest)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"AssertStatusCode() Failure: Values differ{Environment.NewLine}" +
                             $"Expected: {HttpStatusCode.BadRequest}{Environment.NewLine}Actual: {response.StatusCode}{Environment.NewLine}Body:{body}";
            throw new XunitException(message);
        }

        try
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.NotEmpty(problemDetails.Errors);
            return problemDetails;
        }
        catch (Exception e)
        {
            string body = await response.Content.ReadAsStringAsync();
            string message = $"ProblemResponse() Failure: Values differ{Environment.NewLine}" +
                             $"Expected HttpValidationProblemDetails{Environment.NewLine}Actual Body:{body}";
            throw new XunitException(message, e);
        }
    }

    private static void AssertProblemDetails(ProblemDetails problem, int statusCode, string type)
    {
        Assert.Equal(statusCode, problem.Status);
        Assert.Equal(type, problem.Type);
        Assert.NotNull(problem.Title);
        Assert.NotEmpty(problem.Title);
    }
}
