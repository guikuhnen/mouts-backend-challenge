using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

/// <summary>
/// Command for cancelling an entire sale.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="CancelSaleResult"/> that confirms the cancellation.
/// </summary>
public class CancelSaleCommand : IRequest<CancelSaleResult>
{
    /// <summary>Gets the unique identifier of the sale to cancel.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Initializes a new <see cref="CancelSaleCommand"/> for the specified sale.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to cancel.</param>
    public CancelSaleCommand(Guid id)
    {
        Id = id;
    }
}
