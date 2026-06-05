using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Handler for processing <see cref="ListSalesCommand"/> requests.
/// Retrieves a paginated, date-ordered list of sales from the repository and maps
/// each record to a <see cref="ListSaleItemResult"/>.
/// </summary>
public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="ListSalesHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to retrieve the paginated sales list.</param>
    /// <param name="mapper">AutoMapper instance for projecting domain entities to result DTOs.</param>
    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the <see cref="ListSalesCommand"/> by fetching the requested page of sales
    /// and assembling the pagination metadata.
    /// </summary>
    /// <param name="command">The command specifying the page number and page size.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="ListSalesResult"/> containing the sales for the requested page,
    /// total record count, and derived pagination data.
    /// </returns>
    public async Task<ListSalesResult> Handle(ListSalesCommand command, CancellationToken cancellationToken)
    {
        var (sales, totalCount) = await _saleRepository.GetAllAsync(command.Page, command.PageSize, cancellationToken);

        return new ListSalesResult
        {
            Sales = _mapper.Map<IEnumerable<ListSaleItemResult>>(sales),
            TotalCount = totalCount,
            Page = command.Page,
            PageSize = command.PageSize
        };
    }
}
