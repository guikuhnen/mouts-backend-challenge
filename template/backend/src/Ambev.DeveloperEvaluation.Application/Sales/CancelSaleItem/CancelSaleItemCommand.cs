using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

/// <summary>
/// Command for cancelling a single item within an active sale.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="CancelSaleItemResult"/> that confirms the item cancellation.
/// The parent sale remains active; only the specified item is cancelled.
/// </summary>
public class CancelSaleItemCommand : IRequest<CancelSaleItemResult>
{
    /// <summary>Gets the unique identifier of the parent sale.</summary>
    public Guid SaleId { get; set; }

    /// <summary>Gets the unique identifier of the item to cancel.</summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Initializes a new <see cref="CancelSaleItemCommand"/> for the specified sale and item.
    /// </summary>
    /// <param name="saleId">The unique identifier of the parent sale.</param>
    /// <param name="itemId">The unique identifier of the item to cancel.</param>
    public CancelSaleItemCommand(Guid saleId, Guid itemId)
    {
        SaleId = saleId;
        ItemId = itemId;
    }
}
