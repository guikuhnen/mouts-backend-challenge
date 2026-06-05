namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when an entire <see cref="Entities.Sale"/> is cancelled.
/// Published as a structured log entry and can be forwarded to a message broker in future integrations.
/// </summary>
public class SaleCancelledEvent
{
    /// <summary>Gets the unique identifier of the cancelled sale.</summary>
    public Guid SaleId { get; }

    /// <summary>Gets the sequential sale number of the cancelled sale.</summary>
    public int SaleNumber { get; }

    /// <summary>Gets the UTC date and time when this event was raised.</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new <see cref="SaleCancelledEvent"/> with the details of the cancelled sale.
    /// </summary>
    /// <param name="saleId">The unique identifier of the sale.</param>
    /// <param name="saleNumber">The sequential sale number.</param>
    public SaleCancelledEvent(Guid saleId, int saleNumber)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        OccurredAt = DateTime.UtcNow;
    }
}
