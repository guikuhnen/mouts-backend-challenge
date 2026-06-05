using AutoMapper;
using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Handler for processing <see cref="GetSaleCommand"/> requests.
/// Retrieves a sale aggregate from the repository and maps it to a <see cref="GetSaleResult"/>.
/// </summary>
public class GetSaleHandler : IRequestHandler<GetSaleCommand, GetSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="GetSaleHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to retrieve the sale.</param>
    /// <param name="mapper">AutoMapper instance for converting the domain entity to a result DTO.</param>
    public GetSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Handles the <see cref="GetSaleCommand"/> by loading the sale and its items from the repository.
    /// </summary>
    /// <param name="command">The command containing the sale identifier to look up.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="GetSaleResult"/> with the sale data and all its items.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no sale exists with the given identifier.</exception>
    public async Task<GetSaleResult> Handle(GetSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        return _mapper.Map<GetSaleResult>(sale);
    }
}
