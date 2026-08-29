namespace ImageDrop.Persistence.Extensions;

public static partial class ModelBuilderExtensions
{
    public static ModelBuilder UseUpperSnakeCaseColumnNames(this ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        foreach (IMutableProperty property in entity.GetProperties())
            property.SetColumnName(ToSnakeCase(property.Name).ToUpperInvariant());

        return modelBuilder;
    }

    private static string ToSnakeCase(string value)
        => UpperSnakeCaseRegex().Replace(value, "_$1");

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex UpperSnakeCaseRegex();
}