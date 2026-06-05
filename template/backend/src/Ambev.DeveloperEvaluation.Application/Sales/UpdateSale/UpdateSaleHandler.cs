using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

/// <summary>
/// Handler for processing <see cref="UpdateSaleCommand"/> requests.
/// Validates the command, loads the existing sale, replaces its items and header fields,
/// persists the changes and publishes a <see cref="SaleModifiedEvent"/>.
/// </summary>
public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateSaleHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to load and persist the sale.</param>
    /// <param name="mapper">AutoMapper instance for converting the domain entity to a result DTO.</param>
    /// <param name="logger">Logger used to publish the <see cref="SaleModifiedEvent"/> as a structured log entry.</param>
    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the <see cref="UpdateSaleCommand"/> by updating the sale's header and replacing
    /// all its items with the new set provided in the command.
    /// </summary>
    /// <param name="command">The update command with the new sale data and items.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="UpdateSaleResult"/> with the updated sale details.</returns>
    /// <exception cref="ValidationException">Thrown when the command fails validation rules.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when no sale exists with the given identifier.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to update a cancelled sale.</exception>
    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with ID {command.Id} not found.");

        if (sale.IsCancelled)
            throw new InvalidOperationException("Cannot update a cancelled sale.");

        sale.Update(command.CustomerName, command.BranchName, command.SaleDate);
        sale.CustomerId = command.CustomerId;
        sale.BranchId = command.BranchId;

        var items = command.Items.Select(i => new SaleItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();
        sale.SetItems(items);

        var updated = await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new SaleModifiedEvent(updated.Id, updated.SaleNumber, updated.TotalAmount);
        _logger.LogInformation("SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}, Total={TotalAmount}",
            domainEvent.SaleId, domainEvent.SaleNumber, domainEvent.TotalAmount);

        return _mapper.Map<UpdateSaleResult>(updated);
    }
}
