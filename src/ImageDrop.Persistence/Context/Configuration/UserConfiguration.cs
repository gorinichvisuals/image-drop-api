namespace ImageDrop.Persistence.Context.Configuration;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("USERS");
        
        builder.HasKey(user => user.Id);
        
        builder.Property(user => user.Nickname)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.Property(user => user.Password)
            .IsRequired();
        
        builder.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(user => user.Email)
            .IsUnique();
        
        builder.Property(user => user.Role)
            .IsRequired()
            .HasConversion<string>();
        
        builder.HasMany(user => user.Images)
            .WithOne(image => image.User)
            .HasForeignKey(image => image.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}