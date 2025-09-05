using Microsoft.EntityFrameworkCore;
using Saithis.TestApi.Db;
using Saithis.TestApi.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<DummyDbContext>(c => c.UseSqlite());

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

TestEndpoint.Register(app);

app.Run();

namespace Saithis.TestApi
{
    public abstract partial class Program;
}
