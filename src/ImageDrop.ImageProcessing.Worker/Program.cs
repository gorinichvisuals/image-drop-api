HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
await builder.Configuration.AddSecretsToConfiguration($"{environment}_image_drop_secrets");

builder.Services.AddSqsConsumer(builder.Configuration);
builder.Services.AddHostedService<ImageProcessingWorker>();

IHost host = builder.Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{    
    logger.LogInformation("ImageDrop Image Processing Worker started.");

    await host.RunAsync();
}
catch (OperationCanceledException)
{
    logger.LogInformation("ImageDrop Image Processing Worker stopped.");
}
catch (Exception exception)
{
    logger.LogCritical(exception, "ImageDrop Image Processing Worker terminated unexpectedly.");
}