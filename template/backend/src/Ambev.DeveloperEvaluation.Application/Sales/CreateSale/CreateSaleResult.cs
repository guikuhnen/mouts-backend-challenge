namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Represents the result returned by <see cref="CreateSaleHandler"/> after a sale is successfully created.
/// Contains the full sale record including all items with their computed discounts and totals.
/// </summary>
public class CreateSaleResult
{
    /// <summary>Gets or sets the unique identifier of the created sale.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the sequential sale number assigned at creation time.</summary>
    public int SaleNumber { get; set; }

    /// <summary>Gets or sets the date and time when the sale transaction occurred.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>Gets or sets the external identifier of the customer.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>Gets or sets the denormalized customer name recorded on the sale.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the external identifier of the branch.</summary>
    public Guid BranchId { get; set; }

    /// <summary>Gets or sets the denormalized branch name recorded on the sale.</summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the total monetary value of the sale, computed as the sum of
    /// all item totals after discounts are applied.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Gets or sets a value indicating whether the sale has been cancelled.</summary>
    public bool IsCancelled { get; set; }

    /// <summary>Gets or sets the list of items included in the sale.</summary>
    public List<CreateSaleItemResult> Items { get; set; } = new();
}

/// <summary>
/// Represents the result for a single item within a <see cref="CreateSaleResult"/>.
/// </summary>
public class CreateSaleItemResult
{
    /// <summary>Gets or sets the unique identifier of the sale item.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the external identifier of the product.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the denormalized product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of units sold.</summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the price per unit.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the discount rate applied to this item (e.g. 0.10 for 10%, 0.20 for 20%).
    /// Determined automatically by quantity-based business rules.
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Gets or sets the total amount for this item after discount:
    /// <c>UnitPrice × Quantity × (1 − Discount)</c>.
    /// </summary>
    public decimal TotalAmount { get; set; }
}
