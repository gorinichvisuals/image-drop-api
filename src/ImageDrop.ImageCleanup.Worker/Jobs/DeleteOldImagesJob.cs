namespace ImageDrop.ImageCleanup.Worker.Jobs;

internal sealed class DeleteOldImagesJob(
    IImageProcessingService imageProcessingService, 
    ILogger<DeleteOldImagesJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Starting old images cleanup.");

            await imageProcessingService.ImagesCleanup();
            
            logger.LogInformation("Old images cleanup complete.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error while executing job {jobName}.", nameof(DeleteOldImagesJob));
            
            throw;
        }
    }
}