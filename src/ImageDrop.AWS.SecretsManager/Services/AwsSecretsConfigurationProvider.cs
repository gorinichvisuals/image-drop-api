namespace ImageDrop.AWS.SecretsManager.Services;

public sealed class AwsSecretsConfigurationProvider(string json) : ConfigurationProvider
{
    public override void Load()
    {
        using JsonDocument document = JsonDocument.Parse(json);

        LoadElement(document.RootElement, null);
    }

    private void LoadElement(JsonElement element, string? parentPath)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    string key = parentPath is null
                        ? property.Name
                        : $"{parentPath}:{property.Name}";

                    LoadElement(property.Value, key);
                }

                break;

            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in element.EnumerateArray())
                    LoadElement(item, $"{parentPath}:{index++}");

                break;

            default:
                Data[parentPath!] = element.ToString();
                break;
        }
    }
}