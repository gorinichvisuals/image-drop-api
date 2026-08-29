namespace ImageDrop.Persistence.Repositories.UoW;

public interface IUnitOfWork
{
    Task Save();
    IUserRepository UserRepository { get; }
    IImageRepository ImageRepository { get; }
}