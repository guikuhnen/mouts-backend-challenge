using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Handler for processing <see cref="CancelSaleCommand"/> requests.
/// Marks the entire sale as cancelled, persists the change and publishes a <see cref="SaleCancelledEvent"/>.
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, CancelSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CancelSaleHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to load and persist the sale.</param>
    /// <param name="mapper">AutoMapper instance for converting the domain entity to a result DTO.</param>
    /// <param name="logger">Logger used to publish the <see cref="SaleCancelledEvent"/> as a structured log entry.</param>
    public CancelSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<CancelSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the <see cref="CancelSaleCommand"/> by loading the sale, calling
    /// <see cref="Domain.Entities.Sale.Cancel"/> and persisting the updated state.
    /// </summary>
    /// <param name="command">The command containing the identifier of the sale to cancel.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="CancelSaleResult"/> confirming the cancellation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no sale exists with the given identifier.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the sale is already cancelled.</exception>
    public async Task<CancelSaleResult> Handle(CancelSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Sale is already cancelled.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new SaleCancelledEvent(sale.Id, sale.SaleNumber);
        _logger.LogInformation("SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}",
            domainEvent.SaleId, domainEvent.SaleNumber);

        return _mapper.Map<CancelSaleResult>(sale);
    }
}
