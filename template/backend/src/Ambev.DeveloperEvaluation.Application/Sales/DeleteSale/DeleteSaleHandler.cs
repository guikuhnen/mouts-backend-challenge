using MediatR;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

/// <summary>
/// Handler for processing <see cref="DeleteSaleCommand"/> requests.
/// Delegates permanent deletion to the repository and returns a success indicator.
/// </summary>
public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResult>
{
    private readonly ISaleRepository _saleRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteSaleHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to permanently remove the sale.</param>
    public DeleteSaleHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    /// <summary>
    /// Handles the <see cref="DeleteSaleCommand"/> by instructing the repository to delete the sale.
    /// </summary>
    /// <param name="command">The command containing the identifier of the sale to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="DeleteSaleResult"/> with <c>Success = true</c>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no sale exists with the given identifier.</exception>
    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var deleted = await _saleRepository.DeleteAsync(command.Id, cancellationToken);

        if (!deleted)
            throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        return new DeleteSaleResult { Success = true };
    }
}
