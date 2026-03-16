namespace CodePulse.PostService.Domain.Entities;

/// <summary>
/// Tag is a Value Object — no identity, equality based on Name.
/// </summary>
public sealed class Tag
{
    public string Name { get; private set; } = string.Empty;

    // ── EF Core ──
    private Tag() { }

    public static Tag Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalized = name.Trim().ToLowerInvariant();

        if (normalized.Length > 50)
            throw new ArgumentException("Tag name must be 50 characters or less.");

        return new Tag { Name = normalized };
    }

    // ── Value Object equality ──────────────────────
    public override bool Equals(object? obj)
        => obj is Tag other && Name == other.Name;

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString() => Name;
}
