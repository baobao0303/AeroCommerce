using AeroCommerce.PostService.Domain.Entities;

namespace AeroCommerce.PostService.Domain.Interfaces;

/// <summary>
/// Repository contract for Post — defined in Domain, implemented in Infrastructure.
/// </summary>
public interface IPostRepository
{
    Task<IReadOnlyList<Post>> GetAllPublishedAsync(CancellationToken ct = default);
    Task<Post?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Post?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Post post, CancellationToken ct = default);
    Task RemoveAsync(Post post, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
