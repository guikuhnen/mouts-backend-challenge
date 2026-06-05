using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _logger);
    }

    [Fact(DisplayName = "Given valid sale data When creating sale Then returns success response")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = 1 };
        var result = new CreateSaleResult { Id = sale.Id, SaleNumber = 1 };

        _saleRepository.GetNextSaleNumberAsync(Arg.Any<CancellationToken>()).Returns(1);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<CreateSaleResult>(sale).Returns(result);

        // When
        var createSaleResult = await _handler.Handle(command, CancellationToken.None);

        // Then
        createSaleResult.Should().NotBeNull();
        createSaleResult.Id.Should().Be(sale.Id);
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given invalid sale data When creating sale Then throws validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand(); // Empty command fails validation

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given sale with items above 20 When creating Then throws InvalidOperationException")]
    public async Task Handle_ItemQuantityAbove20_ThrowsException()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand(1);
        command.Items[0].Quantity = 21;
        _saleRepository.GetNextSaleNumberAsync(Arg.Any<CancellationToken>()).Returns(1);

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given valid sale When creating Then sale number is assigned")]
    public async Task Handle_ValidRequest_AssignsSaleNumber()
    {
        // Given
        var command = CreateSaleHandlerTestData.GenerateValidCommand();
        var expectedSaleNumber = 42;
        var sale = new Sale { Id = Guid.NewGuid(), SaleNumber = expectedSaleNumber };
        var result = new CreateSaleResult { Id = sale.Id, SaleNumber = expectedSaleNumber };

        _saleRepository.GetNextSaleNumberAsync(Arg.Any<CancellationToken>()).Returns(expectedSaleNumber);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());
        _mapper.Map<CreateSaleResult>(Arg.Any<Sale>()).Returns(result);

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).GetNextSaleNumberAsync(Arg.Any<CancellationToken>());
        await _saleRepository.Received(1).CreateAsync(
            Arg.Is<Sale>(s => s.SaleNumber == expectedSaleNumber),
            Arg.Any<CancellationToken>());
    }
}
