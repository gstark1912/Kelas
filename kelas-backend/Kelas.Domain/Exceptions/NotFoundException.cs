namespace Kelas.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, string id)
        : base($"{entity} con id '{id}' no encontrado.") { }
}
