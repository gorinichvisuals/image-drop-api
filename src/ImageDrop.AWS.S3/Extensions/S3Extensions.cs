namespace ImageDrop.AWS.S3.Extensions;

public static class S3Extensions
{
    extension(IServiceCollection services)
    {
        public void AddS3Services(IConfiguration configuration)
        {        
            AWSCoreOptions coreOptions = configuration.GetSection(nameof(AWSCoreOptions)).Get<AWSCoreOptions>()!;

            services.AddSingleton<IAmazonS3>(_ =>
            {
                AmazonS3Config config = new()
                {
                    RegionEndpoint = coreOptions.Region,
                    ForcePathStyle = true
                };

                BasicAWSCredentials credentials = new(coreOptions.AccessKey, coreOptions.SecretKey);

                return new AmazonS3Client(credentials, config);
            });
        
            services.AddTransient<IStorageService, StorageService>();
        }
    }
}