namespace ImageDrop.AWS.SQS.Publisher.Extensions;

public static class SqsPublisherExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSqsServices(IConfiguration configuration)
        {
            AWSCoreOptions coreOptions = configuration.GetSection(nameof(AWSCoreOptions)).Get<AWSCoreOptions>()!;
            
            services.AddSingleton<IAmazonSQS>(_ =>
            {
                AmazonSQSConfig config = new()
                {
                    RegionEndpoint = coreOptions.Region,
                };
                
                BasicAWSCredentials credentials = new(coreOptions.AccessKey, coreOptions.SecretKey);

                return new AmazonSQSClient(credentials, config);
            });
            
            services.AddScoped<ISqsPublisher, SqsPublisher>();
        }
    }
}