using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Generic service interface for Dataverse entity operations.
/// Provides business logic layer over the repository pattern.
/// </summary>
/// <typeparam name="T">The entity type that implements IDataverseEntity.</typeparam>
public interface IDataverseService<T> where T : IDataverseEntity, new()
{
    /// <summary>
    /// Creates a new entity with validation.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The created entity with updated ID.</returns>
    Task<T> CreateAsync(T entity);
    
    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="columns">Specific columns to retrieve.</param>
    /// <returns>The retrieved entity, or null if not found.</returns>
    Task<T?> GetByIdAsync(Guid id, params string[] columns);
    
    /// <summary>
    /// Updates an existing entity with validation.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>True if the entity was deleted, false if it didn't exist.</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Searches for entities based on criteria.
    /// </summary>
    /// <param name="attributeName">The attribute name to search on.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="operator">The search operator.</param>
    /// <param name="columns">Specific columns to retrieve.</param>
    /// <returns>A collection of matching entities.</returns>
    Task<IEnumerable<T>> SearchAsync(string attributeName, object searchValue, ConditionOperator @operator = ConditionOperator.Equal, params string[] columns);
    
    /// <summary>
    /// Gets all entities with optional filtering.
    /// </summary>
    /// <param name="query">Optional query expression for advanced filtering.</param>
    /// <returns>A collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync(QueryExpression? query = null);
    
    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Guid id);
}