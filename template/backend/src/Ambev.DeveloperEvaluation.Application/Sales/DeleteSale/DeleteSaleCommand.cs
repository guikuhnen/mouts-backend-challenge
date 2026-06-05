using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Command for permanently deleting a sale record from the data store.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="DeleteSaleResult"/> confirming the deletion.
/// </summary>
public class DeleteSaleCommand : IRequest<DeleteSaleResult>
{
    /// <summary>Gets the unique identifier of the sale to delete.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Initializes a new <see cref="DeleteSaleCommand"/> for the specified sale.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    public DeleteSaleCommand(Guid id)
    {
        Id = id;
    }
}
