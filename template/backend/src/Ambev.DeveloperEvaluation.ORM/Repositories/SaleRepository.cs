using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="ISaleRepository"/>.
/// Provides CRUD operations and pagination for the <see cref="Sale"/> aggregate,
/// always loading the <see cref="Sale.Items"/> collection eagerly.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="SaleRepository"/>.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new <see cref="Sale"/> to the database and saves changes.
    /// </summary>
    /// <param name="sale">The sale aggregate to persist.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The persisted <see cref="Sale"/>.</returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Retrieves a <see cref="Sale"/> by its unique identifier, including all <see cref="Sale.Items"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="Sale"/>, or <c>null</c> if not found.</returns>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a <see cref="Sale"/> by its sequential sale number, including all <see cref="Sale.Items"/>.
    /// </summary>
    /// <param name="saleNumber">The unique sale number.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="Sale"/>, or <c>null</c> if not found.</returns>
    public async Task<Sale?> GetBySaleNumberAsync(int saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    /// <summary>
    /// Returns a page of sales ordered by <see cref="Sale.SaleDate"/> descending,
    /// along with the total record count for pagination metadata.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple of the sales for the requested page and the total number of sales in the store.
    /// </returns>
    public async Task<(IEnumerable<Sale> Sales, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales.Include(s => s.Items).AsQueryable();
        var totalCount = await query.CountAsync(cancellationToken);
        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (sales, totalCount);
    }

    /// <summary>
    /// Updates an existing <see cref="Sale"/> in the database and saves changes.
    /// </summary>
    /// <param name="sale">The sale aggregate with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="Sale"/>.</returns>
    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        _context.Sales.Update(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Permanently removes a <see cref="Sale"/> and its cascaded items from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the sale was found and deleted; <c>false</c> otherwise.</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FindAsync(new object[] { id }, cancellationToken);
        if (sale == null) return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Calculates the next available sequential sale number by querying the current maximum.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The next sale number (current maximum + 1, or 1 if no sales exist).</returns>
    public async Task<int> GetNextSaleNumberAsync(CancellationToken cancellationToken = default)
    {
        var maxNumber = await _context.Sales.MaxAsync(s => (int?)s.SaleNumber, cancellationToken);
        return (maxNumber ?? 0) + 1;
    }
}
