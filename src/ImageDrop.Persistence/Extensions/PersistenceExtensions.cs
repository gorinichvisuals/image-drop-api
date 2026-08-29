namespace ImageDrop.Persistence.Extensions;

public static class PersistenceExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPersistenceRepositories()
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
        }

        public void ConfigureSqlConnection(IConfiguration configuration)
        {
            services.AddDbContext<ImageDropContext>((_, options) =>
            {
                DbConnectionOptions dbConnectionOptions = configuration.GetSection(nameof(DbConnectionOptions)).Get<DbConnectionOptions>()!;
            
                options.UseNpgsql(dbConnectionOptions.ImageDropConnectionString);
            });
        }
    }
}