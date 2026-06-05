using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Handler for processing <see cref="CancelSaleItemCommand"/> requests.
/// Cancels a single item within an active sale, recalculates the sale total and
/// publishes an <see cref="ItemCancelledEvent"/>.
/// </summary>
public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CancelSaleItemHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to load and persist the sale.</param>
    /// <param name="logger">Logger used to publish the <see cref="ItemCancelledEvent"/> as a structured log entry.</param>
    public CancelSaleItemHandler(ISaleRepository saleRepository, ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the <see cref="CancelSaleItemCommand"/> by locating the item within the sale,
    /// calling <see cref="Domain.Entities.Sale.CancelItem"/> and persisting the updated state.
    /// </summary>
    /// <param name="command">The command specifying which sale and item to cancel.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="CancelSaleItemResult"/> confirming the item cancellation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the sale or item does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the parent sale is already cancelled.</exception>
    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.SaleId} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Cannot cancel an item of a cancelled sale.");

        var item = sale.Items.FirstOrDefault(i => i.Id == command.ItemId)
            ?? throw new KeyNotFoundException($"Item {command.ItemId} not found in sale {command.SaleId}.");

        sale.CancelItem(command.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new ItemCancelledEvent(sale.Id, item.Id, item.ProductId, item.ProductName);
        _logger.LogInformation("ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, Product={ProductName}",
            domainEvent.SaleId, domainEvent.ItemId, domainEvent.ProductName);

        return new CancelSaleItemResult
        {
            SaleId = sale.Id,
            ItemId = command.ItemId,
            IsCancelled = true
        };
    }
}
