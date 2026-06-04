namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleResult
{
    public Guid Id { get; set; }
    public int SaleNumber { get; set; }
    public bool IsCancelled { get; set; }
}
