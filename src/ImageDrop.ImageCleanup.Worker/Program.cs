HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

string environment =  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

await builder.Configuration.AddSecretsToConfiguration($"{environment}_image_drop_secrets");

builder.Services.AddSharedServices();
builder.Services.AddPersistenceRepositories();
builder.Services.AddSharedServicesOptions(builder.Configuration);
builder.Services.AddS3Services(builder.Configuration);
builder.Services.ConfigureSqlConnection(builder.Configuration);

ImageCleanupOptions imageCleanupOptions = builder.Configuration.GetSection(nameof(ImageCleanupOptions)).Get<ImageCleanupOptions>()!;

builder.Services.AddQuartz(options =>
{
    JobKey jobKey = new(nameof(DeleteOldImagesJob));
    
    options.AddJob<DeleteOldImagesJob>(jobKey);
    
    options.AddTrigger(trigger => trigger
        .ForJob(jobKey)
        .WithIdentity(imageCleanupOptions.TriggerIdentity)
        .WithSimpleSchedule(scheduleBuilder => scheduleBuilder.WithIntervalInHours(imageCleanupOptions.IntervalInHours).RepeatForever()));
});

builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

IHost host = builder.Build();

ILogger<Program> logger = host.Services.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("ImageDrop ImageCleanup Worker started.");

    await host.RunAsync();
}
catch (OperationCanceledException)
{
    logger.LogInformation("ImageDrop ImageCleanup Worker stopped.");
}
catch (Exception exception)
{
    logger.LogError(exception, "ImageDrop ImageCleanup Worker terminated unexpectedly.");
}