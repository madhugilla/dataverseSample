using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Generic service implementation for Dataverse entity operations.
/// Provides business logic layer over the repository pattern.
/// </summary>
/// <typeparam name="T">The entity type that implements IDataverseEntity.</typeparam>
public class DataverseService<T> : IDataverseService<T> where T : IDataverseEntity, new()
{
    private readonly IRepository<T> _repository;

    /// <summary>
    /// Initializes a new instance of the DataverseService class.
    /// </summary>
    /// <param name="repository">The repository for data access.</param>
    public DataverseService(IRepository<T> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Creates a new entity with validation.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The created entity with updated ID.</returns>
    public async Task<T> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Validate entity before creating
        ValidateEntity(entity);

        // Create the entity
        var newId = await _repository.CreateAsync(entity);
        entity.Id = newId;

        return entity;
    }

    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="columns">Specific columns to retrieve.</param>
    /// <returns>The retrieved entity, or null if not found.</returns>
    public async Task<T?> GetByIdAsync(Guid id, params string[] columns)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));

        ColumnSet? columnSet = null;
        if (columns != null && columns.Length > 0)
        {
            columnSet = new ColumnSet(columns);
        }

        return await _repository.RetrieveAsync(id, columnSet);
    }

    /// <summary>
    /// Updates an existing entity with validation.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty for update operation", nameof(entity));

        // Validate entity before updating
        ValidateEntity(entity);

        // Check if entity exists
        var exists = await _repository.ExistsAsync(entity.Id);
        if (!exists)
        {
            throw new InvalidOperationException($"Entity with ID {entity.Id} does not exist");
        }

        // Update the entity
        await _repository.UpdateAsync(entity);

        return entity;
    }

    /// <summary>
    /// Deletes an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>True if the entity was deleted, false if it didn't exist.</returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));

        // Check if entity exists before deleting
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
        {
            return false;
        }

        await _repository.DeleteAsync(id);
        return true;
    }

    /// <summary>
    /// Searches for entities based on criteria.
    /// </summary>
    /// <param name="attributeName">The attribute name to search on.</param>
    /// <param name="searchValue">The value to search for.</param>
    /// <param name="operator">The search operator.</param>
    /// <param name="columns">Specific columns to retrieve.</param>
    /// <returns>A collection of matching entities.</returns>
    public async Task<IEnumerable<T>> SearchAsync(string attributeName, object searchValue, ConditionOperator @operator = ConditionOperator.Equal, params string[] columns)
    {
        if (string.IsNullOrEmpty(attributeName))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));

        if (searchValue == null)
            throw new ArgumentNullException(nameof(searchValue));

        ColumnSet? columnSet = null;
        if (columns != null && columns.Length > 0)
        {
            columnSet = new ColumnSet(columns);
        }

        return await _repository.RetrieveMultipleAsync(attributeName, @operator, searchValue, columnSet);
    }

    /// <summary>
    /// Gets all entities with optional filtering.
    /// </summary>
    /// <param name="query">Optional query expression for advanced filtering.</param>
    /// <returns>A collection of entities.</returns>
    public async Task<IEnumerable<T>> GetAllAsync(QueryExpression? query = null)
    {
        if (query != null)
        {
            return await _repository.RetrieveMultipleAsync(query);
        }

        // Create a simple query to get all records
        var tempInstance = new T();
        var defaultQuery = new QueryExpression(tempInstance.LogicalName)
        {
            ColumnSet = new ColumnSet(true)
        };

        return await _repository.RetrieveMultipleAsync(defaultQuery);
    }

    /// <summary>
    /// Checks if an entity exists.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _repository.ExistsAsync(id);
    }

    /// <summary>
    /// Validates the entity before create or update operations.
    /// Override this method in derived classes to add specific validation logic.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    protected virtual void ValidateEntity(T entity)
    {
        // Basic validation - can be extended by derived classes
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Add common validation logic here
        // For example, you might want to validate required fields
    }
}