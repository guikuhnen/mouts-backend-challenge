using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesHandler : IRequestHandler<ListSalesCommand, ListSalesResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

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
