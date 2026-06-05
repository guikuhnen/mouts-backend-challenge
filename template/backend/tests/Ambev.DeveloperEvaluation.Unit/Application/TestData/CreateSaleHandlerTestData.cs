using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class CreateSaleHandlerTestData
{
    private static readonly Faker Faker = new();

    public static CreateSaleCommand GenerateValidCommand(int itemCount = 2)
    {
        return new CreateSaleCommand
        {
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Address.City(),
            SaleDate = Faker.Date.Recent(),
            Items = GenerateValidItems(itemCount)
        };
    }

    private static List<CreateSaleItemCommand> GenerateValidItems(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new CreateSaleItemCommand
            {
                ProductId = Guid.NewGuid(),
                ProductName = Faker.Commerce.ProductName(),
                Quantity = Faker.Random.Int(1, 3),
                UnitPrice = Faker.Random.Decimal(10, 500)
            })
            .ToList();
    }
}
