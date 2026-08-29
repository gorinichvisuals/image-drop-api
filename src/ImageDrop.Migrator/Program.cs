using Serilog;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ImageDrop.Persistence.Context;
using ImageDrop.Persistence.Extensions;

using ImageDrop.AWS.SecretsManager.Services;

ServiceCollection services = new();
ConfigurationBuilder configuration = new();

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

await configuration.AddSecretsToConfiguration($"{environment}_image-drop-secrets");

IConfigurationRoot root = configuration.Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(root)
    .WriteTo.Console()
    .CreateLogger();

services.ConfigureSqlConnection(root);

ServiceProvider serviceProvider = services.BuildServiceProvider();

try
{   
    Log.Information("ImageDrop database migrator started.");
    
    await using ImageDropContext context = serviceProvider.GetRequiredService<ImageDropContext>();
    
    Log.Information("Applying ImageDrop database migrations.");
    
    await context.Database.MigrateAsync();
    
    Log.Information("ImageDrop database migrator finished.");
}
catch(Exception exception)
{
    Log.Fatal(exception, "ImageDrop database migrator failed.");
}
finally
{
    await Log.CloseAndFlushAsync();
}