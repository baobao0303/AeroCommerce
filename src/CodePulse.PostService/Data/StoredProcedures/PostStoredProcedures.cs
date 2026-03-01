using CodePulse.PostService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodePulse.PostService.Data.StoredProcedures;

/// <summary>
/// Wrapper gọi Stored Procedures của PostService.
/// </summary>
public class PostStoredProcedures
{
    private readonly PostDbContext _db;

    public PostStoredProcedures(PostDbContext db) => _db = db;

    /// <summary>
    /// SP: Tìm kiếm bài viết theo keyword (full-text search).
    /// </summary>
    public async Task<List<Post>> SearchPostsAsync(string keyword)
    {
        return await _db.Posts
            .FromSqlRaw("SELECT * FROM sp_search_posts({0})", keyword)
            .Include(p => p.Tags)
            .ToListAsync();
    }

    /// <summary>
    /// SP: Lấy bài viết theo tag.
    /// </summary>
    public async Task<List<Post>> GetPostsByTagAsync(string tagName)
    {
        return await _db.Posts
            .FromSqlRaw("SELECT p.* FROM posts p INNER JOIN tags t ON t.post_id = p.id WHERE t.name = {0} AND p.status = 'Published'", tagName)
            .Include(p => p.Tags)
            .ToListAsync();
    }

    /// <summary>
    /// SP: Lấy bài viết phổ biến nhất (theo số lượt xem).
    /// </summary>
    public async Task<List<Post>> GetTrendingPostsAsync(int limit = 10)
    {
        return await _db.Posts
            .FromSqlRaw("SELECT * FROM sp_get_trending_posts({0})", limit)
            .Include(p => p.Tags)
            .ToListAsync();
    }

    /// <summary>
    /// SP: Archive toàn bộ bài của một author.
    /// </summary>
    public async Task<int> ArchiveAuthorPostsAsync(Guid authorId)
    {
        return await _db.Database
            .ExecuteSqlRawAsync("CALL sp_archive_author_posts({0})", authorId);
    }
}
