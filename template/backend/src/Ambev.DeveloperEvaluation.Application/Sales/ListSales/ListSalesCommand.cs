using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Command for retrieving a paginated list of all sales.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="ListSalesResult"/> with the requested page of sales and pagination metadata.
/// </summary>
public class ListSalesCommand : IRequest<ListSalesResult>
{
    /// <summary>
    /// Gets or sets the 1-based page number to retrieve.
    /// Defaults to 1.
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum number of sales to return per page.
    /// Defaults to 10.
    /// </summary>
    public int PageSize { get; set; } = 10;
}
