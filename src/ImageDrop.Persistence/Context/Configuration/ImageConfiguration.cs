namespace ImageDrop.Persistence.Context.Configuration;

internal sealed class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.ToTable("IMAGES");
        
        builder.HasKey(image => image.Id);
        
        builder.Property(image => image.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(image => image.ProcessingStatus)
            .IsRequired()
            .HasConversion<string>();
        
        builder.Property(image => image.Format)
            .IsRequired()
            .HasConversion<string>();
        
        builder.HasOne(image => image.User)
            .WithMany(user => user.Images)
            .HasForeignKey(image => image.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(image => image.UserId);
    }
}