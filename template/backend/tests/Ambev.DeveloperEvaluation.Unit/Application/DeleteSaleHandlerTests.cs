using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "Given existing sale ID When deleting Then returns success")]
    public async Task Handle_ExistingSale_ReturnsSuccess()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        var result = await _handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        await _saleRepository.Received(1).DeleteAsync(id, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given non-existing sale ID When deleting Then throws KeyNotFoundException")]
    public async Task Handle_NonExistingSale_ThrowsKeyNotFoundException()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(false);

        // When
        var act = () => _handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact(DisplayName = "Given valid ID When deleting Then repository delete is called exactly once")]
    public async Task Handle_ValidId_CallsRepositoryOnce()
    {
        // Given
        var id = Guid.NewGuid();
        _saleRepository.DeleteAsync(id, Arg.Any<CancellationToken>()).Returns(true);

        // When
        await _handler.Handle(new DeleteSaleCommand(id), CancellationToken.None);

        // Then
        await _saleRepository.Received(1).DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
