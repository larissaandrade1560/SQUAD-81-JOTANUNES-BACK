namespace JotaNunesForms.Application.Auth;

public abstract class SecurityException(string message) : Exception(message);

public sealed class InvalidIdentityException(string message) : SecurityException(message);

public sealed class InsufficientCapabilityException(string message) : SecurityException(message);

public sealed class ResourceNotFoundException(string message = "Recurso não encontrado.") : SecurityException(message);

public sealed record SecurityRequestContext(string TraceId, string Method, string Route);
