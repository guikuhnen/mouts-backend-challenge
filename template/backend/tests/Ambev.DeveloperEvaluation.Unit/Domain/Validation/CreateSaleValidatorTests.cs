using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class CreateSaleValidatorTests
{
    private readonly CreateSaleCommandValidator _validator = new();

    [Fact(DisplayName = "Given valid sale command When validating Then should pass")]
    public void Given_ValidCommand_When_Validated_Then_ShouldPass()
    {
        // Arrange
        var command = new CreateSaleCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = "ACME Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Main Branch",
            SaleDate = DateTime.UtcNow,
            Items = new List<CreateSaleItemCommand>
            {
                new() { ProductId = Guid.NewGuid(), ProductName = "Widget", Quantity = 5, UnitPrice = 99.99m }
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Given empty CustomerId When validating Then should fail")]
    public void Given_EmptyCustomerId_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.CustomerId = Guid.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact(DisplayName = "Given empty CustomerName When validating Then should fail")]
    public void Given_EmptyCustomerName_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.CustomerName = string.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact(DisplayName = "Given empty BranchId When validating Then should fail")]
    public void Given_EmptyBranchId_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.BranchId = Guid.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.BranchId);
    }

    [Fact(DisplayName = "Given empty Items list When validating Then should fail")]
    public void Given_EmptyItems_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.Items = new List<CreateSaleItemCommand>();
        _validator.TestValidate(command).ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Theory(DisplayName = "Given item quantity out of range When validating Then should fail")]
    [InlineData(0)]
    [InlineData(21)]
    [InlineData(-1)]
    public void Given_ItemQuantityOutOfRange_When_Validated_Then_ShouldHaveError(int quantity)
    {
        var command = BuildValidCommand();
        command.Items[0].Quantity = quantity;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact(DisplayName = "Given item UnitPrice zero or less When validating Then should fail")]
    public void Given_ZeroUnitPrice_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.Items[0].UnitPrice = 0;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }

    [Fact(DisplayName = "Given item empty ProductId When validating Then should fail")]
    public void Given_EmptyProductId_When_Validated_Then_ShouldHaveError()
    {
        var command = BuildValidCommand();
        command.Items[0].ProductId = Guid.Empty;
        _validator.TestValidate(command).ShouldHaveValidationErrorFor("Items[0].ProductId");
    }

    [Fact(DisplayName = "Given multiple valid items When validating Then should pass")]
    public void Given_MultipleValidItems_When_Validated_Then_ShouldPass()
    {
        var command = BuildValidCommand();
        command.Items.Add(new CreateSaleItemCommand
        {
            ProductId = Guid.NewGuid(), ProductName = "Gadget", Quantity = 20, UnitPrice = 50m
        });

        _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
    }

    private static CreateSaleCommand BuildValidCommand() => new()
    {
        CustomerId = Guid.NewGuid(),
        CustomerName = "Test Corp",
        BranchId = Guid.NewGuid(),
        BranchName = "Test Branch",
        SaleDate = DateTime.UtcNow,
        Items = new List<CreateSaleItemCommand>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Product", Quantity = 3, UnitPrice = 10m }
        }
    };
}
