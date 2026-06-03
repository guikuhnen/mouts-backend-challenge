using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; set; }

    public Sale Sale { get; set; } = null!;

    public SaleItem() { }

    public SaleItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    public void SetQuantity(int quantity)
    {
        if (quantity > 20)
            throw new InvalidOperationException("Cannot sell more than 20 identical items.");

        Quantity = quantity;
        Discount = CalculateDiscount(quantity);
        TotalAmount = UnitPrice * quantity * (1 - Discount);
    }

    public void Cancel()
    {
        IsCancelled = true;
    }

    private static decimal CalculateDiscount(int quantity)
    {
        if (quantity >= 10) return 0.20m;
        if (quantity >= 4) return 0.10m;
        return 0m;
    }
}
