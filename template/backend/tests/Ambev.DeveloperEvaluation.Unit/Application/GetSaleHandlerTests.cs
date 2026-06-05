using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale ID When getting sale Then returns sale data")]
    public async Task Handle_ExistingSale_ReturnsSaleResult()
    {
        // Given
        var sale = GetSaleHandlerTestData.GenerateSaleWithItems(3);
        var result = new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            CustomerName = sale.CustomerName,
            TotalAmount = sale.TotalAmount,
            IsCancelled = false,
            Items = new List<GetSaleItemResult>()
        };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // When
        var response = await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        // Then
        response.Should().NotBeNull();
        response.Id.Should().Be(sale.Id);
        response.SaleNumber.Should().Be(sale.SaleNumber);
        await _saleRepository.Received(1).GetByIdAsync(sale.Id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing sale ID When getting sale Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new GetSaleCommand(id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given cancelled sale ID When getting sale Then returns cancelled sale")]
    public async Task Handle_CancelledSale_ReturnsCancelledData()
    {
        // Given
        var sale = GetSaleHandlerTestData.GenerateSaleWithItems(1);
        sale.Cancel();

        var result = new GetSaleResult { Id = sale.Id, IsCancelled = true };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // When
        var response = await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        // Then
        response.IsCancelled.Should().BeTrue();
    }

    [Fact(DisplayName = "Given sale with multiple items When getting Then mapper is called once")]
    public async Task Handle_SaleWithItems_CallsMapperOnce()
    {
        // Given
        var sale = GetSaleHandlerTestData.GenerateSaleWithItems(5);
        var result = new GetSaleResult { Id = sale.Id };

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<GetSaleResult>(sale).Returns(result);

        // When
        await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        // Then
        _mapper.Received(1).Map<GetSaleResult>(sale);
    }
}
