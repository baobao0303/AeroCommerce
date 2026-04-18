namespace AeroCommerce.PostService.Domain.Exceptions;

public class PostNotFoundException : DomainException
{
    public PostNotFoundException(Guid id)
        : base($"Post with ID '{id}' was not found.") { }
}

public class SlugAlreadyExistsException : DomainException
{
    public SlugAlreadyExistsException(string slug)
        : base($"A post with slug '{slug}' already exists.") { }
}

public class UnauthorizedPostAccessException : DomainException
{
    public UnauthorizedPostAccessException()
        : base("You are not authorized to modify this post.") { }
}

/// <summary>Base class for all domain exceptions in PostService.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
