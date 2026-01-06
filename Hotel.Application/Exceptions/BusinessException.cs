namespace Hotel.Application.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}

public class NotFoundException : BusinessException
{
    public NotFoundException(string entity, int id)
        : base($"{entity} with ID {id} was not found.") { }

    public NotFoundException(string entity, string identifier)
        : base($"{entity} '{identifier}' was not found.") { }
}

public class ValidationException : BusinessException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error) : base(error)
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.AsReadOnly();
    }
}

public class ConflictException : BusinessException
{
    public ConflictException(string message) : base(message) { }
}

public class RoomNotAvailableException : ConflictException
{
    public RoomNotAvailableException(int roomId, DateTime checkIn, DateTime checkOut)
        : base($"Room {roomId} is not available from {checkIn:yyyy-MM-dd} to {checkOut:yyyy-MM-dd}.") { }
}
