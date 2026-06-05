using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

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
