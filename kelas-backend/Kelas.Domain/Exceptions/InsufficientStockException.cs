using Kelas.Domain.Models.Responses;

namespace Kelas.Domain.Exceptions;

public class InsufficientStockException : BusinessException
{
    public InsufficientStockException(List<InsufficientStockItemResponse> items)
        : base("Stock insuficiente para una o más materias primas.")
    {
        Items = items;
    }

    public List<InsufficientStockItemResponse> Items { get; }
}
