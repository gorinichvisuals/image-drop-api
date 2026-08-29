namespace ImageDrop.Shared.Services.Extensions;

public static class SharedServicesExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSharedServices()
        {
            services.AddScoped<IImageCompressorService, ImageCompressorService>();
            services.AddScoped<IImageProcessingService, ImageProcessingService>();
        }

        public void AddSharedServicesOptions(IConfiguration configuration)
        {
            services.Configure<AWSCoreOptions>(configuration.GetSection(nameof(AWSCoreOptions)));
            services.Configure<S3Options>(configuration.GetSection(nameof(S3Options)));
            services.Configure<SqsOptions>(configuration.GetSection(nameof(SqsOptions)));
            services.Configure<ImageProcessingOptions>(configuration.GetSection(nameof(ImageProcessingOptions)));
            services.Configure<ImageCleanupOptions>(configuration.GetSection(nameof(ImageCleanupOptions)));
        }
    }
}