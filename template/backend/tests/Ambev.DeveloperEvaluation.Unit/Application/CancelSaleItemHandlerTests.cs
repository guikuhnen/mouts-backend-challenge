using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _logger = Substitute.For<ILogger<CancelSaleItemHandler>>();
        _handler = new CancelSaleItemHandler(_saleRepository, _logger);
    }

    [Fact(DisplayName = "Given active item When cancelling Then item is marked cancelled")]
    public async Task Handle_ActiveItem_CancelsItem()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Product X", 5, 100m);
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = 1 };
        sale.SetItems(new[] { item });

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, item.Id);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.IsCancelled.Should().BeTrue();
        result.SaleId.Should().Be(sale.Id);
        result.ItemId.Should().Be(item.Id);
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        // Given
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var command = new CancelSaleItemCommand(saleId, Guid.NewGuid());

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{saleId}*");
    }

    [Fact(DisplayName = "Given non-existing item When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingItem_ThrowsKeyNotFoundException()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid() };
        sale.SetItems(new[] { new SaleItem(Guid.NewGuid(), "Product", 1, 10m) });

        var nonExistingItemId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, nonExistingItemId);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{nonExistingItemId}*");
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws InvalidOperationException")]
    public async Task Handle_CancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var item = new SaleItem(Guid.NewGuid(), "Product", 1, 10m);
        var sale = new Sale { Id = Guid.NewGuid() };
        sale.SetItems(new[] { item });
        sale.Cancel();

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, item.Id);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cancelled sale*");
    }

    [Fact(DisplayName = "Given sale with multiple items When cancelling one Then only that item is cancelled")]
    public async Task Handle_MultipleItems_CancelsOnlyTargetItem()
    {
        // Given
        var itemToCancel = new SaleItem(Guid.NewGuid(), "Product A", 2, 50m);
        var otherItem = new SaleItem(Guid.NewGuid(), "Product B", 3, 80m);
        var sale = new Sale { Id = Guid.NewGuid() };
        sale.SetItems(new[] { itemToCancel, otherItem });

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, itemToCancel.Id);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        sale.Items.First(i => i.Id == itemToCancel.Id).IsCancelled.Should().BeTrue();
        sale.Items.First(i => i.Id == otherItem.Id).IsCancelled.Should().BeFalse();
    }
}
