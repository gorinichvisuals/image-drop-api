namespace ImageDrop.Persistence.Repositories.Implementations;

internal sealed class ImageRepository(ImageDropContext context) : IImageRepository
{
    public ImageDropContext Context { get; set; } = context;

    public async Task UpdateImageProcessingStatus(Guid guidImageId, ImageProcessingStatus  imageProcessingStatus) =>
        await Context.Images
            .Where(image => image.Id == guidImageId)
            .ExecuteUpdateAsync(image => image.SetProperty(property => property.ProcessingStatus, imageProcessingStatus));
}