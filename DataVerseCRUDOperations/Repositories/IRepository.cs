using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Repositories;

/// <summary>
/// Generic repository interface for Dataverse entity operations.
/// Provides type-safe CRUD operations for early bound entities.
/// </summary>
/// <typeparam name="T">The entity type that implements IDataverseEntity.</typeparam>
public interface IRepository<T> where T : IDataverseEntity, new()
{
    /// <summary>
    /// Creates a new entity in Dataverse.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The ID of the created entity.</returns>
    Task<Guid> CreateAsync(T entity);
    
    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="columnSet">The columns to retrieve. If null, all columns are retrieved.</param>
    /// <returns>The retrieved entity, or null if not found.</returns>
    Task<T?> RetrieveAsync(Guid id, ColumnSet? columnSet = null);
    
    /// <summary>
    /// Updates an existing entity in Dataverse.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity from Dataverse.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Retrieves multiple entities based on a query expression.
    /// </summary>
    /// <param name="query">The query expression to execute.</param>
    /// <returns>A collection of entities matching the query.</returns>
    Task<IEnumerable<T>> RetrieveMultipleAsync(QueryExpression query);
    
    /// <summary>
    /// Retrieves multiple entities based on a simple condition.
    /// </summary>
    /// <param name="attributeName">The attribute name to filter on.</param>
    /// <param name="conditionOperator">The condition operator.</param>
    /// <param name="value">The value to compare against.</param>
    /// <param name="columnSet">The columns to retrieve. If null, all columns are retrieved.</param>
    /// <returns>A collection of entities matching the condition.</returns>
    Task<IEnumerable<T>> RetrieveMultipleAsync(string attributeName, ConditionOperator conditionOperator, object value, ColumnSet? columnSet = null);
    
    /// <summary>
    /// Checks if an entity exists in Dataverse.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Guid id);
    
    /// <summary>
    /// Executes multiple operations in a single transaction.
    /// </summary>
    /// <param name="requests">The collection of organization requests to execute.</param>
    /// <returns>The responses from the executed requests.</returns>
    Task<ExecuteTransactionResponse> ExecuteTransactionAsync(IEnumerable<OrganizationRequest> requests);
}