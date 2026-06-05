using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a sales transaction in the DeveloperStore domain.
/// Acts as the aggregate root for the sale, owning and coordinating a collection of
/// <see cref="SaleItem"/> objects. Enforces business rules such as discount calculation,
/// cancellation and total amount computation.
/// </summary>
public class Sale : BaseEntity
{
    /// <summary>
    /// Gets or sets the sequential sale number assigned at creation time.
    /// Unique across all sales and used as a human-readable reference.
    /// </summary>
    public int SaleNumber { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the sale transaction occurred.
    /// </summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the external identifier of the customer who made the purchase.
    /// Follows the External Identity pattern — the customer belongs to another domain.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized customer name captured at the time of sale.
    /// Stored locally to preserve the sale record even if the customer data changes externally.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the external identifier of the branch where the sale was made.
    /// Follows the External Identity pattern — the branch belongs to another domain.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized branch name captured at the time of sale.
    /// Stored locally to preserve the sale record even if the branch data changes externally.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the total monetary value of the sale, calculated as the sum of all non-cancelled
    /// <see cref="SaleItem.TotalAmount"/> values. Updated automatically whenever items are
    /// added, replaced or cancelled.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this sale has been cancelled.
    /// Cancelled sales cannot be updated and their items cannot be individually cancelled.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Gets the date and time when this sale record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the date and time of the last update to this sale record.
    /// Null if the sale has never been modified after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();

    /// <summary>
    /// Gets the read-only collection of items belonging to this sale.
    /// Use <see cref="AddItem"/> or <see cref="SetItems"/> to modify the collection.
    /// </summary>
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Initializes a new <see cref="Sale"/> with the current UTC date and time.
    /// </summary>
    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        SaleDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Appends a single <see cref="SaleItem"/> to this sale and recalculates <see cref="TotalAmount"/>.
    /// Sets the item's <see cref="SaleItem.SaleId"/> to the current sale's identifier.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(SaleItem item)
    {
        item.SaleId = Id;
        _items.Add(item);
        RecalculateTotal();
    }

    /// <summary>
    /// Replaces all existing items with the provided collection and recalculates <see cref="TotalAmount"/>.
    /// Sets each item's <see cref="SaleItem.SaleId"/> to the current sale's identifier.
    /// </summary>
    /// <param name="items">The new set of items for this sale.</param>
    public void SetItems(IEnumerable<SaleItem> items)
    {
        _items.Clear();
        foreach (var item in items)
        {
            item.SaleId = Id;
            _items.Add(item);
        }
        RecalculateTotal();
    }

    /// <summary>
    /// Marks the entire sale as cancelled and records the cancellation timestamp.
    /// Cancelled sales cannot be updated or have their items cancelled individually.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels a specific item within the sale by its identifier and recalculates
    /// <see cref="TotalAmount"/> to exclude the cancelled item.
    /// </summary>
    /// <param name="itemId">The unique identifier of the item to cancel.</param>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no item with the given <paramref name="itemId"/> exists in this sale.
    /// </exception>
    public void CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} not found in sale.");
        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the mutable header fields of this sale and records the update timestamp.
    /// Does not modify the sale's items.
    /// </summary>
    /// <param name="customerName">The updated customer name.</param>
    /// <param name="branchName">The updated branch name.</param>
    /// <param name="saleDate">The updated sale date.</param>
    public void Update(string customerName, string branchName, DateTime saleDate)
    {
        CustomerName = customerName;
        BranchName = branchName;
        SaleDate = saleDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Recalculates <see cref="TotalAmount"/> as the sum of <see cref="SaleItem.TotalAmount"/>
    /// for all non-cancelled items. Called internally whenever the item collection changes.
    /// </summary>
    private void RecalculateTotal()
    {
        TotalAmount = _items
            .Where(i => !i.IsCancelled)
            .Sum(i => i.TotalAmount);
    }
}
