using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddSso();

builder.Build().Run();

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> AddSso(this IDistributedApplicationBuilder builder)
    {
        return builder.AddProject<SsoApi>("sso");
    }
}