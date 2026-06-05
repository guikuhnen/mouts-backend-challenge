using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelSaleHandler> _logger;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CancelSaleHandler>>();
        _handler = new CancelSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then sale is cancelled")]
    public async Task Handle_ActiveSale_CancelsSale()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = 1 };
        var result = new CancelSaleResult { Id = sale.Id, SaleNumber = 1, IsCancelled = true };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CancelSaleResult>(sale).Returns(result);

        // When
        var cancelResult = await _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        // Then
        cancelResult.Should().NotBeNull();
        cancelResult.IsCancelled.Should().BeTrue();
        await _saleRepository.Received(1).UpdateAsync(Arg.Is<Sale>(s => s.IsCancelled), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new CancelSaleCommand(id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given already cancelled sale When cancelling Then throws InvalidOperationException")]
    public async Task Handle_AlreadyCancelledSale_ThrowsInvalidOperationException()
    {
        // Given
        var sale = new Sale { Id = Guid.NewGuid() };
        sale.Cancel();

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var act = () => _handler.Handle(new CancelSaleCommand(sale.Id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }
}
