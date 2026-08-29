namespace ImageDrop.Application.Extensions;

public static class ApplicationExtensions
{
    extension(IServiceCollection services)
    {
        public void AddApplicationServices()
        {        
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICacheService, CacheService>();
        }

        public IConnectionMultiplexer AddRedisCache(IConfiguration configuration)
        {
            RedisOptions redisOptions = configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>()!;
            
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisOptions.Configuration);

            services.AddSingleton(connectionMultiplexer);
            
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.Configuration;
                options.InstanceName = redisOptions.InstanceName;
            });
            
            return connectionMultiplexer;
        }
    }
}