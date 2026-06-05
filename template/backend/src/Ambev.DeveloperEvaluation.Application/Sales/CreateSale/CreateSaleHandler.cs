using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Handler for processing <see cref="CreateSaleCommand"/> requests.
/// Orchestrates validation, aggregate construction, persistence and domain event publication.
/// </summary>
public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateSaleHandler"/>.
    /// </summary>
    /// <param name="saleRepository">Repository used to persist the new sale.</param>
    /// <param name="mapper">AutoMapper instance for converting between domain and result types.</param>
    /// <param name="logger">Logger used to publish the <see cref="SaleCreatedEvent"/> as a structured log entry.</param>
    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Handles the <see cref="CreateSaleCommand"/> by validating the command, building the
    /// <see cref="Sale"/> aggregate with its items (including discount calculation), persisting
    /// it and publishing a <see cref="SaleCreatedEvent"/>.
    /// </summary>
    /// <param name="command">The create sale command containing customer, branch and item data.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="CreateSaleResult"/> with the persisted sale details.</returns>
    /// <exception cref="ValidationException">Thrown when the command fails validation rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any item quantity exceeds 20.</exception>
    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleCommandValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var saleNumber = await _saleRepository.GetNextSaleNumberAsync(cancellationToken);

        var sale = new Sale
        {
            SaleNumber = saleNumber,
            SaleDate = command.SaleDate,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            BranchId = command.BranchId,
            BranchName = command.BranchName
        };

        var items = command.Items.Select(i => new SaleItem(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList();
        sale.SetItems(items);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        var domainEvent = new SaleCreatedEvent(created.Id, created.SaleNumber, created.CustomerId, created.CustomerName, created.TotalAmount);
        _logger.LogInformation("SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, Customer={CustomerName}, Total={TotalAmount}",
            domainEvent.SaleId, domainEvent.SaleNumber, domainEvent.CustomerName, domainEvent.TotalAmount);

        return _mapper.Map<CreateSaleResult>(created);
    }
}
