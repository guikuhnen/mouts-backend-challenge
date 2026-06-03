namespace Ambev.DeveloperEvaluation.Domain.Events;

public class SaleCreatedEvent
{
    public Guid SaleId { get; }
    public int SaleNumber { get; }
    public Guid CustomerId { get; }
    public string CustomerName { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredAt { get; }

    public SaleCreatedEvent(Guid saleId, int saleNumber, Guid customerId, string customerName, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        CustomerId = customerId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}
