namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesResult
{
    public IEnumerable<ListSaleItemResult> Sales { get; set; } = Enumerable.Empty<ListSaleItemResult>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ListSaleItemResult
{
    public Guid Id { get; set; }
    public int SaleNumber { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsCancelled { get; set; }
}
