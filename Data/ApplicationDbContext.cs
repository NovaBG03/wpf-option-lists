using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WpfApp.Domain.Entities;

namespace WpfApp.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Activity> Activities { get; set; }
    public DbSet<Technology> Technologies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<BaseEntity>();

        modelBuilder.Entity<User>(user =>
        {
            ConfigureBaseEntity(user);

            user.HasOne(u => u.FavouriteActivity)
                .WithMany(a => a.Users);

            user.HasOne(u => u.FavouriteTechnology)
                .WithMany(t => t.Users);
        });

        modelBuilder.Entity<Activity>(activity =>
        {
            ConfigureBaseEntity(activity);
            activity.HasIndex(a => a.Name).IsUnique();
        });

        modelBuilder.Entity<Technology>(technology =>
        {
            ConfigureBaseEntity(technology);
            technology.HasIndex(t => t.Name).IsUnique();
        });
    }

    private static void ConfigureBaseEntity<T>(EntityTypeBuilder<T> baseEntity) where T : BaseEntity
    {
        baseEntity.HasKey(e => e.Id);
    }
}
