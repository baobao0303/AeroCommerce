using AeroCommerce.PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AeroCommerce.PostService.Data;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Supabase: dùng schema riêng tránh xung đột với UserService
        modelBuilder.HasDefaultSchema("post_service");

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Title).HasMaxLength(500).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(500).IsRequired();
            entity.Property(p => p.AuthorName).HasMaxLength(200);
            entity.Property(p => p.Status).HasConversion<string>();

            // Tags as owned collection
            entity.OwnsMany(p => p.Tags, tag =>
            {
                tag.WithOwner().HasForeignKey("PostId");
                tag.Property(t => t.Name).HasMaxLength(50).IsRequired();
            });
        });
    }
}
