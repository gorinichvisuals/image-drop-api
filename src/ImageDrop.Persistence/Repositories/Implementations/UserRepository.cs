namespace ImageDrop.Persistence.Repositories.Implementations;

internal sealed class UserRepository(ImageDropContext context) : IUserRepository
{
    public ImageDropContext Context { get; set; } = context;
}