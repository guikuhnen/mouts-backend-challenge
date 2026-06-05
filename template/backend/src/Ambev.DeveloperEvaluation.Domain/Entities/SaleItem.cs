using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

/// <summary>
/// Represents a single product line within a <see cref="Sale"/>.
/// Encapsulates quantity-based discount calculation and total amount computation
/// according to the business rules defined for the DeveloperStore sales domain.
/// </summary>
public class SaleItem : BaseEntity
{
    /// <summary>
    /// Gets or sets the identifier of the parent <see cref="Sale"/>.
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// Gets or sets the external identifier of the product being sold.
    /// Follows the External Identity pattern — the product belongs to another domain.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the denormalized product name captured at the time of sale.
    /// Stored locally so the sale record remains accurate even if the product is renamed later.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of units of this product in the sale.
    /// Must be between 1 and 20. Changing the quantity via <see cref="SetQuantity"/> recalculates
    /// the applicable discount and total amount.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per unit of the product at the time of sale.
    /// Must be greater than zero.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the discount rate applied to this item, expressed as a decimal fraction (e.g. 0.10 for 10%).
    /// Determined automatically by <see cref="CalculateDiscount"/> based on <see cref="Quantity"/>:
    /// <list type="bullet">
    ///   <item>1–3 items: 0% (no discount)</item>
    ///   <item>4–9 items: 10%</item>
    ///   <item>10–20 items: 20%</item>
    /// </list>
    /// </summary>
    public decimal Discount { get; set; }

    /// <summary>
    /// Gets the computed total amount for this item after applying the discount.
    /// Calculated as: <c>UnitPrice × Quantity × (1 − Discount)</c>.
    /// This property is recalculated automatically whenever <see cref="SetQuantity"/> is called.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether this item has been individually cancelled.
    /// Cancelled items are excluded from the parent sale's total amount.
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Gets or sets the parent <see cref="Sale"/> navigation property.
    /// </summary>
    public Sale Sale { get; set; } = null!;

    /// <summary>
    /// Parameterless constructor required by Entity Framework Core.
    /// </summary>
    public SaleItem() { }

    /// <summary>
    /// Initializes a new <see cref="SaleItem"/> with a generated identifier and the provided product details.
    /// The discount and total amount are calculated immediately based on the given quantity.
    /// </summary>
    /// <param name="productId">The external identifier of the product.</param>
    /// <param name="productName">The denormalized product name to store on the sale record.</param>
    /// <param name="quantity">The number of units. Must be between 1 and 20.</param>
    /// <param name="unitPrice">The price per unit. Must be greater than zero.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="quantity"/> exceeds 20.
    /// </exception>
    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    /// <summary>
    /// Updates the quantity of this item and recalculates the applicable discount and total amount.
    /// </summary>
    /// <param name="quantity">The new quantity. Must be between 1 and 20.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="quantity"/> exceeds 20, as selling more than 20 identical items
    /// is not permitted by the business rules.
    /// </exception>
    public void SetQuantity(int quantity)
    {
        if (quantity > 20)
            throw new InvalidOperationException("Cannot sell more than 20 identical items.");

        Quantity = quantity;
        Discount = CalculateDiscount(quantity);
        TotalAmount = UnitPrice * quantity * (1 - Discount);
    }

    /// <summary>
    /// Marks this item as cancelled. Cancelled items are excluded from the parent sale's
    /// <see cref="Sale.TotalAmount"/> recalculation.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
    }

    /// <summary>
    /// Determines the discount rate for a given quantity according to the business rules.
    /// </summary>
    /// <param name="quantity">The quantity to evaluate.</param>
    /// <returns>
    /// 0.20 for quantities of 10 or more;
    /// 0.10 for quantities of 4 to 9;
    /// 0.00 for quantities below 4.
    /// </returns>
    private static decimal CalculateDiscount(int quantity)
    {
        if (quantity >= 10) return 0.20m;
        if (quantity >= 4) return 0.10m;
        return 0m;
    }
}
