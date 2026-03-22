using CodePulse.PostService.Data;
using CodePulse.PostService.Domain.Entities;
using CodePulse.PostService.Domain.Exceptions;
using CodePulse.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CodePulse.PostService.Services;

public interface IPostService
{
    Task<List<PostListDto>> GetAllAsync();
    Task<PostDto?> GetBySlugAsync(string slug);
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<PostDto> CreateAsync(CreatePostRequest request, Guid authorId, string authorName);
    Task<PostDto?> UpdateAsync(Guid id, UpdatePostRequest request, Guid authorId);
    Task<bool> DeleteAsync(Guid id, Guid authorId);
}

public class PostService : IPostService
{
    private readonly PostDbContext _db;

    public PostService(PostDbContext db) => _db = db;

    public async Task<List<PostListDto>> GetAllAsync() =>
        await _db.Posts
            .Include(p => p.Tags)
            .Where(p => p.Status == PostStatus.Published)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostListDto(
                p.Id, p.Title, p.Slug,
                p.Tags.Select(t => t.Name).ToList(),
                p.AuthorId, p.AuthorName, p.CreatedAt))
            .ToListAsync();

    public async Task<PostDto?> GetBySlugAsync(string slug)
    {
        var p = await _db.Posts
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.Status == PostStatus.Published);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        var p = await _db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : MapToDto(p);
    }

    public async Task<PostDto> CreateAsync(CreatePostRequest request, Guid authorId, string authorName)
    {
        if (await _db.Posts.AnyAsync(p => p.Slug == request.Slug))
            throw new SlugAlreadyExistsException(request.Slug);

        // Use domain factory method
        var post = Post.Create(request.Title, request.Content, request.Slug,
                               authorId, authorName, request.Tags);
        post.Publish();

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();
        return MapToDto(post);
    }

    public async Task<PostDto?> UpdateAsync(Guid id, UpdatePostRequest request, Guid authorId)
    {
        var post = await _db.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return null;
        if (post.AuthorId != authorId) throw new UnauthorizedPostAccessException();

        // Use domain behavior method
        post.Update(request.Title, request.Content, request.Tags);
        await _db.SaveChangesAsync();
        return MapToDto(post);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid authorId)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post is null) return false;
        if (post.AuthorId != authorId) throw new UnauthorizedPostAccessException();

        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
        return true;
    }

    private static PostDto MapToDto(Post p) =>
        new(p.Id, p.Title, p.Content, p.Slug,
            p.Tags.Select(t => t.Name).ToList(),
            p.AuthorId, p.AuthorName, p.CreatedAt, p.UpdatedAt);
}
