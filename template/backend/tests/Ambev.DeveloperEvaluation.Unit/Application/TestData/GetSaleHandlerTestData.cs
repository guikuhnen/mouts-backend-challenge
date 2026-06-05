using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.TestData;

public static class GetSaleHandlerTestData
{
    private static readonly Faker Faker = new();

    public static Sale GenerateSaleWithItems(int itemCount = 2)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = Faker.Random.Int(1, 9999),
            SaleDate = Faker.Date.Recent(),
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Address.City()
        };

        var items = Enumerable.Range(0, itemCount)
            .Select(_ => new SaleItem(Guid.NewGuid(), Faker.Commerce.ProductName(), Faker.Random.Int(1, 3), Faker.Random.Decimal(10, 200)))
            .ToList();

        sale.SetItems(items);
        return sale;
    }
}
