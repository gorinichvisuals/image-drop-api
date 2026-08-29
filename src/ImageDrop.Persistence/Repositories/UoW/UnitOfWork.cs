namespace ImageDrop.Persistence.Repositories.UoW;

internal sealed class UnitOfWork(
    ImageDropContext context, 
    IUserRepository userRepository, 
    IImageRepository imageRepository) : IUnitOfWork
{
    private readonly ImageDropContext _context = context;
    
    public IUserRepository UserRepository { get; set; } = userRepository;
    public IImageRepository ImageRepository { get; set; } = imageRepository;
    
    public async Task Save() => await _context.SaveChangesAsync();
}