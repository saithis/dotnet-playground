using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var hub = builder.AddHub();
var sso = builder.AddSso();

builder.AddProject<HostGateway>("host-gateway")
    .WithUrl("http://hub.zvoove-local.cloud", "hub")
    .WithUrl("http://sso.zvoove-local.cloud", "sso")
    .WithReference(hub)
    .WithReference(sso);

builder.Build().Run();
