namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a single <see cref="Entities.SaleItem"/> is cancelled
/// within an active <see cref="Entities.Sale"/>.
/// Published as a structured log entry and can be forwarded to a message broker in future integrations.
/// </summary>
public class ItemCancelledEvent
{
    /// <summary>Gets the unique identifier of the parent sale.</summary>
    public Guid SaleId { get; }

    /// <summary>Gets the unique identifier of the cancelled item.</summary>
    public Guid ItemId { get; }

    /// <summary>Gets the external identifier of the product associated with the cancelled item.</summary>
    public Guid ProductId { get; }

    /// <summary>Gets the denormalized product name of the cancelled item.</summary>
    public string ProductName { get; }

    /// <summary>Gets the UTC date and time when this event was raised.</summary>
    public DateTime OccurredAt { get; }

    /// <summary>
    /// Initializes a new <see cref="ItemCancelledEvent"/> with the details of the cancelled item.
    /// </summary>
    /// <param name="saleId">The unique identifier of the parent sale.</param>
    /// <param name="itemId">The unique identifier of the cancelled item.</param>
    /// <param name="productId">The external product identifier.</param>
    /// <param name="productName">The product name at time of sale.</param>
    public ItemCancelledEvent(Guid saleId, Guid itemId, Guid productId, string productName)
    {
        SaleId = saleId;
        ItemId = itemId;
        ProductId = productId;
        ProductName = productName;
        OccurredAt = DateTime.UtcNow;
    }
}
