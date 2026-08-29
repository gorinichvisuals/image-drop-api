namespace ImageDrop.AWS.SQS.Consumer.Extensions;

public static class SqsConsumerExtensions
{
    extension(IServiceCollection services)
    {
        public void AddSqsConsumer(IConfiguration configuration)
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
            
            services.AddSingleton<IImageDropHelper, ImageDropHelper>();
            
            services.AddScoped<IMessageDispatcher, MessageDispatcher>();
            services.AddScoped<ISqsConsumerService, SqsConsumerService>();
            
            services.AddS3Services(configuration);
            services.ConfigureSqlConnection(configuration);
            services.AddSharedServicesOptions(configuration);
            
            services.AddSharedServices();
            
            services.AddPersistenceRepositories();

            services.AddImageDropMessageHandlers();
        }
        
        private void AddImageDropMessageHandlers()
        {
            Assembly assembly = typeof(ImageDropCreateImageHandler).Assembly;

            IEnumerable<Type> handlers = assembly
                .DefinedTypes
                .Where(type =>
                    typeof(IMessageHandler).IsAssignableFrom(type) &&
                    !type.IsInterface &&
                    !type.IsAbstract)
                .Select(type => type.AsType());

            foreach (Type handler in handlers)
                services.AddScoped(handler);
        }
    }
}