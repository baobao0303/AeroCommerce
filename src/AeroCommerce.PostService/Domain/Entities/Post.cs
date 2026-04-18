namespace AeroCommerce.PostService.Domain.Entities;

/// <summary>
/// Represents a blog post / article.
/// </summary>
public class Post
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public PostStatus Status { get; private set; } = PostStatus.Draft;

    // ── Author info (denormalized — avoid cross-service joins) ──
    public Guid AuthorId { get; private set; }
    public string AuthorName { get; private set; } = string.Empty;

    // ── Tags stored as value objects ──
    private readonly List<Tag> _tags = [];
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    private Post() { }

    // ── Factory method ────────────────────────────
    public static Post Create(string title, string content, string slug,
                              Guid authorId, string authorName,
                              IEnumerable<string>? tags = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var post = new Post
        {
            Title      = title.Trim(),
            Content    = content.Trim(),
            Slug       = slug.Trim().ToLowerInvariant(),
            AuthorId   = authorId,
            AuthorName = authorName
        };

        if (tags is not null)
            foreach (var t in tags)
                post.AddTag(t);

        return post;
    }

    // ── Behavior methods ──────────────────────────
    public void Update(string title, string content, IEnumerable<string> tags)
    {
        Title   = title.Trim();
        Content = content.Trim();
        _tags.Clear();
        foreach (var t in tags) AddTag(t);
        Touch();
    }

    public void Publish()
    {
        if (Status == PostStatus.Published) return;
        Status      = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        Touch();
    }

    public void Unpublish() { Status = PostStatus.Draft; Touch(); }

    public void Archive()   { Status = PostStatus.Archived; Touch(); }

    private void AddTag(string name)
    {
        var tag = Tag.Create(name);
        if (!_tags.Any(t => t.Name == tag.Name))
            _tags.Add(tag);
    }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}

public enum PostStatus
{
    Draft,
    Published,
    Archived
}
