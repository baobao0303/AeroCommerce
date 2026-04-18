using AeroCommerce.PostService.Services;
using AeroCommerce.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AeroCommerce.PostService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService) => _postService = postService;

    /// <summary>Get all published posts</summary>
    [HttpGet]
    public async Task<ActionResult<List<PostListDto>>> GetAll() =>
        Ok(await _postService.GetAllAsync());

    /// <summary>Get post by slug</summary>
    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<PostDto>> GetBySlug(string slug)
    {
        var post = await _postService.GetBySlugAsync(slug);
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>Get post by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostDto>> GetById(Guid id)
    {
        var post = await _postService.GetByIdAsync(id);
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>Create a new post (requires auth)</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest request)
    {
        try
        {
            var (authorId, authorName) = GetAuthorInfo();
            var post = await _postService.CreateAsync(request, authorId, authorName);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Update a post (requires auth, must be author)</summary>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PostDto>> Update(Guid id, [FromBody] UpdatePostRequest request)
    {
        var (authorId, _) = GetAuthorInfo();
        var post = await _postService.UpdateAsync(id, request, authorId);
        return post is null ? NotFound() : Ok(post);
    }

    /// <summary>Delete a post (requires auth, must be author)</summary>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var (authorId, _) = GetAuthorInfo();
        var deleted = await _postService.DeleteAsync(id, authorId);
        return deleted ? NoContent() : NotFound();
    }

    private (Guid authorId, string authorName) GetAuthorInfo()
    {
        var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value ?? throw new UnauthorizedAccessException();
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Unknown";
        return (Guid.Parse(sub), name);
    }
}
