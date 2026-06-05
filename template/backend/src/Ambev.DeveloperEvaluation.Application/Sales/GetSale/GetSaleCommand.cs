using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

/// <summary>
/// Command for retrieving a single sale by its unique identifier.
/// Implements <see cref="IRequest{TResponse}"/> to participate in the MediatR pipeline,
/// returning a <see cref="GetSaleResult"/> with the full sale details including items.
/// </summary>
public class GetSaleCommand : IRequest<GetSaleResult>
{
    /// <summary>Gets the unique identifier of the sale to retrieve.</summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Initializes a new <see cref="GetSaleCommand"/> for the specified sale.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to retrieve.</param>
    public GetSaleCommand(Guid id)
    {
        Id = id;
    }
}
