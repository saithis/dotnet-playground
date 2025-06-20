using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddHub();

builder.Build().Run();

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<ProjectResource> AddHub(this IDistributedApplicationBuilder builder)
    {
        return builder.AddProject<HubApi>("hub");
    }
}