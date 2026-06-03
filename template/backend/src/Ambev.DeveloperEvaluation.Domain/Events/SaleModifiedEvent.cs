namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleModifiedEvent
{
    public Guid SaleId { get; }
    public int SaleNumber { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public SaleModifiedEvent(Guid saleId, int saleNumber, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}
