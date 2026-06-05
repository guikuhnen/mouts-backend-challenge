using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new ListSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given sales exist When listing Then returns paginated result")]
    public async Task Handle_SalesExist_ReturnsPaginatedResult()
    {
        // Given
        var sales = Enumerable.Range(0, 5).Select(_ => GetSaleHandlerTestData.GenerateSaleWithItems()).ToList();
        var mappedItems = sales.Select(s => new ListSaleItemResult { Id = s.Id, SaleNumber = s.SaleNumber }).ToList();
        var command = new ListSalesCommand { Page = 1, PageSize = 10 };

        _saleRepository.GetAllAsync(1, 10, Arg.Any<CancellationToken>()).Returns((sales.AsEnumerable(), sales.Count));
        _mapper.Map<IEnumerable<ListSaleItemResult>>(Arg.Any<IEnumerable<Sale>>()).Returns(mappedItems);

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
    }

    [Fact(DisplayName = "Given no sales When listing Then returns empty result")]
    public async Task Handle_NoSales_ReturnsEmptyResult()
    {
        // Given
        var command = new ListSalesCommand { Page = 1, PageSize = 10 };

        _saleRepository.GetAllAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Sale>(), 0));
        _mapper.Map<IEnumerable<ListSaleItemResult>>(Arg.Any<IEnumerable<Sale>>())
            .Returns(Enumerable.Empty<ListSaleItemResult>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalCount.Should().Be(0);
        result.Sales.Should().BeEmpty();
    }

    [Theory(DisplayName = "Given multiple pages When listing Then pagination is correct")]
    [InlineData(1, 5, 12, 3)]
    [InlineData(2, 5, 12, 3)]
    [InlineData(1, 10, 25, 3)]
    public async Task Handle_Pagination_ReturnsCorrectTotalPages(int page, int pageSize, int totalCount, int expectedTotalPages)
    {
        // Given
        var command = new ListSalesCommand { Page = page, PageSize = pageSize };
        var sales = Enumerable.Empty<Sale>();

        _saleRepository.GetAllAsync(page, pageSize, Arg.Any<CancellationToken>())
            .Returns((sales, totalCount));
        _mapper.Map<IEnumerable<ListSaleItemResult>>(Arg.Any<IEnumerable<Sale>>())
            .Returns(Enumerable.Empty<ListSaleItemResult>());

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.TotalPages.Should().Be(expectedTotalPages);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
    }

    [Fact(DisplayName = "Given listing request When handled Then repository is called with correct pagination params")]
    public async Task Handle_Request_CallsRepositoryWithCorrectParams()
    {
        // Given
        var command = new ListSalesCommand { Page = 3, PageSize = 20 };

        _saleRepository.GetAllAsync(3, 20, Arg.Any<CancellationToken>())
            .Returns((Enumerable.Empty<Sale>(), 0));
        _mapper.Map<IEnumerable<ListSaleItemResult>>(Arg.Any<IEnumerable<Sale>>())
            .Returns(Enumerable.Empty<ListSaleItemResult>());

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _saleRepository.Received(1).GetAllAsync(3, 20, Arg.Any<CancellationToken>());
    }
}
