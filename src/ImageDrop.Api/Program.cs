WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

string environment = builder.Environment.EnvironmentName;

await configuration.AddSecretsToConfiguration($"{environment}_image_drop_secrets");

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCustomValidationResponse();
builder.Services.ConfigureProviders();
builder.Services.AddApiCors();
builder.Services.AddImageDropSwagger();
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceRepositories();
builder.Services.AddSqsServices(configuration);
builder.Services.AddS3Services(configuration);
builder.Services.AddApiAuthentication(configuration);
builder.Services.AddOptions(configuration);
builder.Services.ConfigureSqlConnection(configuration);

IConnectionMultiplexer connectionMultiplexer = builder.Services.AddRedisCache(configuration);
builder.Services.AddImageDropRateLimiter(configuration, connectionMultiplexer);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseImageDropSwagger();

app.UseCors();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRateLimiter();

await app.RunAsync();