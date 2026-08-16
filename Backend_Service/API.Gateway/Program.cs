using Ocelot.DependencyInjection;
using Ocelot.Middleware;

// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();

// app.MapGet("/", () => "Hello World!");

// app.Run();

var builder = WebApplication.CreateBuilder(args);
// 1. Add ocelot.json configuration file
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
//D:\WorkSpace\AzureApps\Entra OIDC_2\Backend_Service\API.Gateway\Ocelot.json

// 2. Register Ocelot services
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

// 3. Use Ocelot middleware
await app.UseOcelot();

app.Run();