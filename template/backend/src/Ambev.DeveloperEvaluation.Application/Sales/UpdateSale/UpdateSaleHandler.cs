using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(ISaleRepository saleRepository, IMapper mapper, ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
        _logger = logger;
    }

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
