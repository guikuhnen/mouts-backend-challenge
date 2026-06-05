using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Command for creating a new sale record.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="CreateSaleResult"/> upon successful creation.
/// </summary>
public class CreateSaleCommand : IRequest<CreateSaleResult>
{
    /// <summary>Gets or sets the external identifier of the customer making the purchase.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized customer name to store on the sale record.
    /// Captured at creation time following the External Identity pattern.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Gets or sets the external identifier of the branch where the sale is made.</summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized branch name to store on the sale record.
    /// Captured at creation time following the External Identity pattern.
    /// </summary>
    public string BranchName { get; set; } = string.Empty;

    /// <summary>Gets or sets the date and time when the sale transaction occurred.</summary>
    public DateTime SaleDate { get; set; }

    /// <summary>
    /// Gets or sets the list of items to include in the sale.
    /// Must contain at least one item. Each item is subject to quantity and price validation.
    /// </summary>
    public List<CreateSaleItemCommand> Items { get; set; } = new();
}

/// <summary>
/// Represents a single product line within a <see cref="CreateSaleCommand"/>.
/// </summary>
public class CreateSaleItemCommand
{
    /// <summary>Gets or sets the external identifier of the product.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the denormalized product name to store on the sale item.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of units to purchase.
    /// Must be between 1 and 20. Values above 20 are rejected by business rules.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>Gets or sets the price per unit. Must be greater than zero.</summary>
    public decimal UnitPrice { get; set; }
}
