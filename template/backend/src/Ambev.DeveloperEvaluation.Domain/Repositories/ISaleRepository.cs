using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Defines the contract for persistence operations on <see cref="Sale"/> aggregates.
/// Implementations are provided by the ORM layer and registered via dependency injection.
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Persists a new <see cref="Sale"/> to the data store.
    /// </summary>
    /// <param name="sale">The sale aggregate to create.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The created <see cref="Sale"/> as stored, including any database-generated values.</returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a <see cref="Sale"/> by its unique identifier, including its <see cref="Sale.Items"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the sale.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="Sale"/>, or <c>null</c> if not found.</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a <see cref="Sale"/> by its sequential sale number, including its <see cref="Sale.Items"/>.
    /// </summary>
    /// <param name="saleNumber">The unique sale number.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The matching <see cref="Sale"/>, or <c>null</c> if not found.</returns>
    Task<Sale?> GetBySaleNumberAsync(int saleNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of all sales, ordered by sale date descending,
    /// along with the total count of records for pagination metadata.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A tuple containing the sales for the requested page and the total record count.
    /// </returns>
    Task<(IEnumerable<Sale> Sales, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing <see cref="Sale"/> in the data store.
    /// </summary>
    /// <param name="sale">The sale aggregate with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated <see cref="Sale"/>.</returns>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently removes a <see cref="Sale"/> and its items from the data store.
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> if the sale was found and deleted; <c>false</c> if it did not exist.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the next available sequential sale number by incrementing the current maximum.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An integer representing the next sale number to assign.</returns>
    Task<int> GetNextSaleNumberAsync(CancellationToken cancellationToken = default);
}
