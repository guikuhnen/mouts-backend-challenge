using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Fact(DisplayName = "Given quantity below 4 When creating item Then no discount is applied")]
    public void Given_QuantityBelow4_When_CreatingItem_Then_NoDiscount()
    {
        // Arrange & Act
        var item = SaleTestData.GenerateItemWithQuantity(3);

        // Assert
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(item.UnitPrice * 3);
    }

    [Theory(DisplayName = "Given quantity 4 to 9 When creating item Then 10% discount is applied")]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(9)]
    public void Given_QuantityBetween4And9_When_CreatingItem_Then_10PercentDiscount(int quantity)
    {
        // Arrange & Act
        var item = SaleTestData.GenerateItemWithQuantity(quantity);

        // Assert
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(item.UnitPrice * quantity * 0.9m);
    }

    [Theory(DisplayName = "Given quantity 10 to 20 When creating item Then 20% discount is applied")]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void Given_QuantityBetween10And20_When_CreatingItem_Then_20PercentDiscount(int quantity)
    {
        // Arrange & Act
        var item = SaleTestData.GenerateItemWithQuantity(quantity);

        // Assert
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(item.UnitPrice * quantity * 0.8m);
    }

    [Fact(DisplayName = "Given quantity above 20 When creating item Then throws InvalidOperationException")]
    public void Given_QuantityAbove20_When_CreatingItem_Then_ThrowsException()
    {
        // Arrange & Act
        var act = () => SaleTestData.GenerateItemWithQuantity(21);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*20*");
    }

    [Fact(DisplayName = "Given active item When cancelled Then IsCancelled is true")]
    public void Given_ActiveItem_When_Cancelled_Then_IsCancelledIsTrue()
    {
        // Arrange
        var item = SaleTestData.GenerateItemWithQuantity(1);

        // Act
        item.Cancel();

        // Assert
        item.IsCancelled.Should().BeTrue();
    }

    [Theory(DisplayName = "Given boundary quantities 1-3 When creating item Then no discount applied")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Given_QuantityBoundary1To3_When_CreatingItem_Then_NoDiscount(int quantity)
    {
        var item = SaleTestData.GenerateItemWithQuantity(quantity);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(item.UnitPrice * quantity);
    }

    [Fact(DisplayName = "Given exact boundary 4 When creating item Then 10% discount starts")]
    public void Given_ExactBoundary4_When_CreatingItem_Then_10PercentDiscount()
    {
        var item = SaleTestData.GenerateItemWithQuantity(4);
        item.Discount.Should().Be(0.10m);
    }

    [Fact(DisplayName = "Given exact boundary 10 When creating item Then 20% discount starts")]
    public void Given_ExactBoundary10_When_CreatingItem_Then_20PercentDiscount()
    {
        var item = SaleTestData.GenerateItemWithQuantity(10);
        item.Discount.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Given exact maximum 20 When creating item Then 20% discount and no exception")]
    public void Given_ExactMaximum20_When_CreatingItem_Then_NoException()
    {
        var act = () => SaleTestData.GenerateItemWithQuantity(20);
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "Given unit price When creating item Then total amount is calculated correctly")]
    public void Given_KnownUnitPrice_When_CreatingItem_Then_TotalAmountIsCorrect()
    {
        // Arrange: 10 items at $50 each => 20% discount => $50 * 10 * 0.8 = $400
        var item = new SaleItem(Guid.NewGuid(), "Test", 10, 50m);

        item.TotalAmount.Should().Be(400m);
        item.Discount.Should().Be(0.20m);
    }

    [Fact(DisplayName = "Given item created When SetQuantity is updated Then discount and total recalculate")]
    public void Given_CreatedItem_When_QuantityUpdated_Then_TotalRecalculates()
    {
        // Arrange: start with 2 items (no discount)
        var item = new SaleItem(Guid.NewGuid(), "Test", 2, 100m);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(200m);

        // Act: update to 10 items (20% discount)
        item.SetQuantity(10);

        // Assert
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(800m); // 10 * 100 * 0.8
    }
}
