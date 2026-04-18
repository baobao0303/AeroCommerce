namespace AeroCommerce.UserService.Domain.Exceptions;

public class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string field, string value)
        : base($"User with {field} '{value}' already exists.") { }
}

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid id)
        : base($"User with ID '{id}' was not found.") { }
}

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("Invalid email or password.") { }
}

/// <summary>Base class for all domain exceptions in UserService.</summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
