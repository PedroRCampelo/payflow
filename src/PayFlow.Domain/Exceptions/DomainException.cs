namespace PayFlow.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} with ID '{id}' was not found.") { }
}