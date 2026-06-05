using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "Given active sale When cancelled Then IsCancelled is true")]
    public void Given_ActiveSale_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        sale.Cancel();

        // Assert
        sale.IsCancelled.Should().BeTrue();
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale with items When SetItems Then TotalAmount is recalculated")]
    public void Given_SaleWithItems_When_SetItems_Then_TotalAmountIsRecalculated()
    {
        // Arrange
        var sale = new Sale { Id = Guid.NewGuid() };
        var item1 = new SaleItem(Guid.NewGuid(), "Product A", 5, 100m); // 10% discount => 450
        var item2 = new SaleItem(Guid.NewGuid(), "Product B", 2, 50m);  // no discount => 100

        // Act
        sale.SetItems(new[] { item1, item2 });

        // Assert
        sale.TotalAmount.Should().Be(550m);
    }

    [Fact(DisplayName = "Given sale with item When item cancelled Then TotalAmount excludes cancelled item")]
    public void Given_SaleWithItems_When_ItemCancelled_Then_TotalAmountExcludesCancelled()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale(0);
        var item1 = new SaleItem(Guid.NewGuid(), "Product A", 2, 100m); // 200
        var item2 = new SaleItem(Guid.NewGuid(), "Product B", 2, 50m);  // 100
        sale.SetItems(new[] { item1, item2 });

        // Act
        sale.CancelItem(item1.Id);

        // Assert
        sale.TotalAmount.Should().Be(100m);
    }

    [Fact(DisplayName = "Given sale When CancelItem with non-existing ID Then throws KeyNotFoundException")]
    public void Given_Sale_When_CancelItemWithInvalidId_Then_ThrowsKeyNotFoundException()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();

        // Act
        var act = () => sale.CancelItem(Guid.NewGuid());

        // Assert
        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given sale When Update Then properties are updated")]
    public void Given_Sale_When_Updated_Then_PropertiesAreUpdated()
    {
        // Arrange
        var sale = SaleTestData.GenerateValidSale();
        var newDate = DateTime.UtcNow.AddDays(1);

        // Act
        sale.Update("New Customer", "New Branch", newDate);

        // Assert
        sale.CustomerName.Should().Be("New Customer");
        sale.BranchName.Should().Be("New Branch");
        sale.SaleDate.Should().Be(newDate);
        sale.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Given sale with no items When SetItems Then TotalAmount is zero")]
    public void Given_SaleWithNoItems_When_SetItems_Then_TotalAmountIsZero()
    {
        var sale = new Sale { Id = Guid.NewGuid() };
        sale.SetItems(Array.Empty<SaleItem>());
        sale.TotalAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "Given sale When AddItem Then TotalAmount increases")]
    public void Given_Sale_When_ItemAdded_Then_TotalIncreases()
    {
        var sale = new Sale { Id = Guid.NewGuid() };
        var item = new SaleItem(Guid.NewGuid(), "Product", 5, 10m); // 5 items, 10% disc => 45

        sale.AddItem(item);

        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "Given sale When all items cancelled Then TotalAmount is zero")]
    public void Given_Sale_When_AllItemsCancelled_Then_TotalAmountIsZero()
    {
        var sale = new Sale { Id = Guid.NewGuid() };
        var item1 = new SaleItem(Guid.NewGuid(), "A", 1, 50m);
        var item2 = new SaleItem(Guid.NewGuid(), "B", 2, 30m);
        sale.SetItems(new[] { item1, item2 });

        sale.CancelItem(item1.Id);
        sale.CancelItem(item2.Id);

        sale.TotalAmount.Should().Be(0m);
    }

    [Fact(DisplayName = "Given sale When AddItem Then item SaleId is set correctly")]
    public void Given_Sale_When_AddItem_Then_ItemSaleIdMatchesSaleId()
    {
        var sale = new Sale { Id = Guid.NewGuid() };
        var item = new SaleItem(Guid.NewGuid(), "Product", 1, 10m);

        sale.AddItem(item);

        item.SaleId.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given sale with discount items When computing total Then discounts are applied")]
    public void Given_SaleWithDiscountItems_When_ComputingTotal_Then_DiscountsApplied()
    {
        // 5 items @$100 => 10% discount => $450
        // 12 items @$50 => 20% discount => $480
        // Total => $930
        var sale = new Sale { Id = Guid.NewGuid() };
        var item1 = new SaleItem(Guid.NewGuid(), "A", 5, 100m);
        var item2 = new SaleItem(Guid.NewGuid(), "B", 12, 50m);
        sale.SetItems(new[] { item1, item2 });

        sale.TotalAmount.Should().Be(930m);
    }

    [Fact(DisplayName = "Given sale When cancelled twice Then still IsCancelled")]
    public void Given_Sale_When_CancelledTwice_Then_StillIsCancelled()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.Cancel();
        sale.Cancel();
        sale.IsCancelled.Should().BeTrue();
    }
}
