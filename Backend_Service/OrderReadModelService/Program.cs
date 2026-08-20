using Azure.Messaging.ServiceBus;
using MongoDB.Driver;
using OrderReadModelService.Models;
using OrderReadModelService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Register MongoDB/Cosmos connection
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["Cosmos:ConnectionString"]
        ?? throw new InvalidOperationException("Cosmos:ConnectionString not configured");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IOrderReadModelRepository, OrderReadModelRepository>();

// Register Service Bus consumer as hosted service
builder.Services.AddHostedService<ServiceBusEventConsumer>();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
