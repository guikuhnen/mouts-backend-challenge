using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSaleHandler> _logger;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<UpdateSaleHandler>>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Given valid sale update When handling Then updates and returns result")]
    public async Task Handle_ValidRequest_UpdatesSale()
    {
        // Given
        var existingSale = new Sale { Id = Guid.NewGuid(), SaleNumber = 1 };
        existingSale.SetItems(new[] { new SaleItem(Guid.NewGuid(), "Product", 1, 100m) });

        var command = new UpdateSaleCommand
        {
            Id = existingSale.Id,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Updated Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Updated Branch",
            SaleDate = DateTime.UtcNow,
            Items = new List<UpdateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "New Product", Quantity = 5, UnitPrice = 200m }
            }
        };

        var result = new UpdateSaleResult { Id = existingSale.Id };

        _saleRepository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>()).Returns(existingSale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(existingSale);
        _mapper.Map<UpdateSaleResult>(existingSale).Returns(result);

        // When
        var updateResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        updateResult.Should().NotBeNull();
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        // Given
        var command = new UpdateSaleCommand
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            SaleDate = DateTime.UtcNow,
            Items = new List<UpdateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 1, UnitPrice = 100m }
            }
        };

        _saleRepository.GetByIdAsync(command.Id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var existingSale = new Sale { Id = Guid.NewGuid() };
        existingSale.Cancel();

        var command = new UpdateSaleCommand
        {
            Id = existingSale.Id,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            SaleDate = DateTime.UtcNow,
            Items = new List<UpdateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 1, UnitPrice = 100m }
            }
        };

        _saleRepository.GetByIdAsync(existingSale.Id, Arg.Any<CancellationToken>()).Returns(existingSale);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
