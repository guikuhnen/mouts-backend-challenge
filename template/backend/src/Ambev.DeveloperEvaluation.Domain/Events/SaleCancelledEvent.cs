namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCancelledEvent
{
    public Guid SaleId { get; }
    public int SaleNumber { get; }
    public DateTime OccurredAt { get; }

    public SaleCancelledEvent(Guid saleId, int saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
