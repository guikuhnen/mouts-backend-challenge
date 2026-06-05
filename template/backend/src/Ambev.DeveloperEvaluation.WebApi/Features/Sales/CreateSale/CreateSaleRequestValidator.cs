using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("Sale must have at least one item.");
        RuleForEach(x => x.Items).SetValidator(new CreateSaleItemRequestValidator());
    }
}

public class CreateSaleItemRequestValidator : AbstractValidator<CreateSaleItemRequest>
{
    public CreateSaleItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Quantity).InclusiveBetween(1, 20);
        RuleFor(x => x.UnitPrice).GreaterThan(0);
    }
}
