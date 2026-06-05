using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker Faker = new();

    public static Sale GenerateValidSale(int itemCount = 2)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = Faker.Random.Int(1, 10000),
            SaleDate = Faker.Date.Recent(),
            CustomerId = Guid.NewGuid(),
            CustomerName = Faker.Company.CompanyName(),
            BranchId = Guid.NewGuid(),
            BranchName = Faker.Address.City()
        };

        var items = GenerateValidItems(itemCount);
        sale.SetItems(items);
        return sale;
    }

    public static List<SaleItem> GenerateValidItems(int count = 2)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new SaleItem(Guid.NewGuid(), Faker.Commerce.ProductName(), Faker.Random.Int(1, 3), Faker.Random.Decimal(1, 100)))
            .ToList();
    }

    public static SaleItem GenerateItemWithQuantity(int quantity)
    {
        return new SaleItem(Guid.NewGuid(), Faker.Commerce.ProductName(), quantity, Faker.Random.Decimal(10, 100));
    }
}
