using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CodePulse.UserService.Data;

/// <summary>
/// Base DbContext — tự động set CreatedAt / UpdatedAt cho mọi entity.
/// UserDbContext kế thừa class này.
/// </summary>
public abstract class ApplicationDbContext : DbContext
{
    protected ApplicationDbContext(DbContextOptions options) : base(options) { }

    /// <summary>
    /// Override SaveChanges — tự động cập nhật audit timestamps.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).CurrentValue = DateTime.UtcNow;
                    entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = null;
                    break;

                case EntityState.Modified:
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false; // không cho sửa
                    entry.Property(nameof(IAuditableEntity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
                    break;
            }
        }
    }

    /// <summary>
    /// Cấu hình chung cho tất cả entities.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tất cả table tên theo snake_case (PostgreSQL convention)
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.ClrType.Name));
        }
    }

    private static string ToSnakeCase(string name) =>
        string.Concat(name.Select((c, i) =>
            i > 0 && char.IsUpper(c) ? "_" + char.ToLower(c) : char.ToLower(c).ToString()));
}

/// <summary>
/// Interface đánh dấu entity có audit timestamps.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
