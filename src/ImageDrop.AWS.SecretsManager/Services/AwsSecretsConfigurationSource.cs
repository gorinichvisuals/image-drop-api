namespace ImageDrop.AWS.SecretsManager.Services;

public sealed class AwsSecretsConfigurationSource(string json) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new AwsSecretsConfigurationProvider(json);
}