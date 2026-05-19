namespace PayFlow.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id)
        : base($"{entity} com ID '{id}' não encontrado(a).") { }
}
