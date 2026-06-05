namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a new <see cref="Entities.Sale"/> is successfully created.
/// Published as a structured log entry and can be forwarded to a message broker in future integrations.
/// </summary>
public class SaleCreatedEvent
{
    /// <summary>Gets the unique identifier of the created sale.</summary>
    public Guid SaleId { get; }

    /// <summary>Gets the sequential sale number assigned to the new sale.</summary>
    public int SaleNumber { get; }

    /// <summary>Gets the external identifier of the customer associated with the sale.</summary>
    public Guid CustomerId { get; }

    /// <summary>Gets the denormalized customer name recorded at creation time.</summary>
    public string CustomerName { get; }

    /// <summary>Gets the total monetary value of the sale at the time of creation.</summary>
    public decimal TotalAmount { get; }

    /// <summary>Gets the UTC date and time when this event was raised.</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new <see cref="SaleCreatedEvent"/> with the details of the created sale.
    /// </summary>
    /// <param name="saleId">The unique identifier of the sale.</param>
    /// <param name="saleNumber">The sequential sale number.</param>
    /// <param name="customerId">The external customer identifier.</param>
    /// <param name="customerName">The customer name at time of sale.</param>
    /// <param name="totalAmount">The total sale amount.</param>
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
