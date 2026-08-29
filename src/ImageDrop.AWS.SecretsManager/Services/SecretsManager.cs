namespace ImageDrop.AWS.SecretsManager.Services;

public static class SecretsManager
{
    public static async Task AddSecretsToConfiguration(this IConfigurationBuilder builder, string secretName)
    {
        using AmazonSecretsManagerClient client = new (RegionEndpoint.EUWest1);

        GetSecretValueResponse response = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretName
        });

        if (string.IsNullOrWhiteSpace(response.SecretString))
            throw new InvalidOperationException("Secret is empty.");

        builder.Add(new AwsSecretsConfigurationSource(response.SecretString));
    }
}