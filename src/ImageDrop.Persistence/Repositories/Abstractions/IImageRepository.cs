namespace ImageDrop.Persistence.Repositories.Abstractions;

public interface IImageRepository : IBaseRepository<Image>
{
    Task UpdateImageProcessingStatus(Guid guidImageId, ImageProcessingStatus  imageProcessingStatus);
}