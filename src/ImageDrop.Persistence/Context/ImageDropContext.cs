namespace ImageDrop.Persistence.Context;

public class ImageDropContext : DbContext 
{
    public DbSet<User>  Users { get; set; }
    public DbSet<Image>  Images { get; set; }
    
    public ImageDropContext()
    {
        
    }
    
    public ImageDropContext(DbContextOptions<ImageDropContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {    
        modelBuilder.UseUpperSnakeCaseColumnNames();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ImageDropContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}