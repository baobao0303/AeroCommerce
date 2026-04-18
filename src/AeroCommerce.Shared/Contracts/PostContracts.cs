namespace AeroCommerce.Shared.Contracts;

// ── Post ──────────────────────────────────────────
public record CreatePostRequest(string Title, string Content, string Slug, List<string> Tags);
public record UpdatePostRequest(string Title, string Content, List<string> Tags);
public record PostDto(Guid Id, string Title, string Content, string Slug, List<string> Tags,
    Guid AuthorId, string AuthorName, DateTime CreatedAt, DateTime? UpdatedAt);
public record PostListDto(Guid Id, string Title, string Slug, List<string> Tags,
    Guid AuthorId, string AuthorName, DateTime CreatedAt);
