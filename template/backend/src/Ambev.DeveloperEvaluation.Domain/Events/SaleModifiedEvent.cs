namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when an existing <see cref="Entities.Sale"/> is successfully updated.
/// Published as a structured log entry and can be forwarded to a message broker in future integrations.
/// </summary>
public class SaleModifiedEvent
{
    /// <summary>Gets the unique identifier of the modified sale.</summary>
    public Guid SaleId { get; }

    /// <summary>Gets the sequential sale number of the modified sale.</summary>
    public int SaleNumber { get; }

    /// <summary>Gets the total monetary value of the sale after the update.</summary>
    public decimal TotalAmount { get; }

    /// <summary>Gets the UTC date and time when this event was raised.</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new <see cref="SaleModifiedEvent"/> with the updated sale details.
    /// </summary>
    /// <param name="saleId">The unique identifier of the sale.</param>
    /// <param name="saleNumber">The sequential sale number.</param>
    /// <param name="totalAmount">The total sale amount after the update.</param>
    public SaleModifiedEvent(Guid saleId, int saleNumber, decimal totalAmount)
    {
        SaleId = saleId;
        SaleNumber = saleNumber;
        TotalAmount = totalAmount;
        OccurredAt = DateTime.UtcNow;
    }
}
