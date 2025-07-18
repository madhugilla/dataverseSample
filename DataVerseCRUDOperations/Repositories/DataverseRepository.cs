using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Repositories;

/// <summary>
/// Generic repository implementation for Dataverse entity operations.
/// Provides type-safe CRUD operations for early bound entities.
/// </summary>
/// <typeparam name="T">The entity type that implements IDataverseEntity.</typeparam>
public class DataverseRepository<T> : IRepository<T> where T : IDataverseEntity, new()
{
    private readonly IOrganizationService _organizationService;
    private readonly string _entityLogicalName;

    /// <summary>
    /// Initializes a new instance of the DataverseRepository class.
    /// </summary>
    /// <param name="organizationService">The Dataverse organization service.</param>
    public DataverseRepository(IOrganizationService organizationService)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
        
        // Get the logical name from a temporary instance
        var tempInstance = new T();
        _entityLogicalName = tempInstance.LogicalName;
    }

    /// <summary>
    /// Creates a new entity in Dataverse.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <returns>The ID of the created entity.</returns>
    public async Task<Guid> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var dataverseEntity = entity.ToEntity();
        
        return await Task.Run(() => _organizationService.Create(dataverseEntity));
    }

    /// <summary>
    /// Retrieves an entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <param name="columnSet">The columns to retrieve. If null, all columns are retrieved.</param>
    /// <returns>The retrieved entity, or null if not found.</returns>
    public async Task<T?> RetrieveAsync(Guid id, ColumnSet? columnSet = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));

        try
        {
            columnSet ??= new ColumnSet(true);
            
            var retrievedEntity = await Task.Run(() => 
                _organizationService.Retrieve(_entityLogicalName, id, columnSet));

            var result = new T();
            result.FromEntity(retrievedEntity);
            return result;
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            return default(T);
        }
    }

    /// <summary>
    /// Updates an existing entity in Dataverse.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty for update operation", nameof(entity));

        var dataverseEntity = entity.ToEntity();
        
        await Task.Run(() => _organizationService.Update(dataverseEntity));
    }

    /// <summary>
    /// Deletes an entity from Dataverse.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));

        await Task.Run(() => _organizationService.Delete(_entityLogicalName, id));
    }

    /// <summary>
    /// Retrieves multiple entities based on a query expression.
    /// </summary>
    /// <param name="query">The query expression to execute.</param>
    /// <returns>A collection of entities matching the query.</returns>
    public async Task<IEnumerable<T>> RetrieveMultipleAsync(QueryExpression query)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));

        if (query.EntityName != _entityLogicalName)
            throw new ArgumentException($"Query entity name '{query.EntityName}' does not match repository entity '{_entityLogicalName}'", nameof(query));

        var results = await Task.Run(() => _organizationService.RetrieveMultiple(query));

        var entities = new List<T>();
        foreach (var entity in results.Entities)
        {
            var result = new T();
            result.FromEntity(entity);
            entities.Add(result);
        }

        return entities;
    }

    /// <summary>
    /// Retrieves multiple entities based on a simple condition.
    /// </summary>
    /// <param name="attributeName">The attribute name to filter on.</param>
    /// <param name="conditionOperator">The condition operator.</param>
    /// <param name="value">The value to compare against.</param>
    /// <param name="columnSet">The columns to retrieve. If null, all columns are retrieved.</param>
    /// <returns>A collection of entities matching the condition.</returns>
    public async Task<IEnumerable<T>> RetrieveMultipleAsync(string attributeName, ConditionOperator conditionOperator, object value, ColumnSet? columnSet = null)
    {
        if (string.IsNullOrEmpty(attributeName))
            throw new ArgumentException("Attribute name cannot be null or empty", nameof(attributeName));

        var query = new QueryExpression(_entityLogicalName)
        {
            ColumnSet = columnSet ?? new ColumnSet(true)
        };
        
        query.Criteria.AddCondition(attributeName, conditionOperator, value);

        return await RetrieveMultipleAsync(query);
    }

    /// <summary>
    /// Checks if an entity exists in Dataverse.
    /// </summary>
    /// <param name="id">The ID of the entity to check.</param>
    /// <returns>True if the entity exists, false otherwise.</returns>
    public async Task<bool> ExistsAsync(Guid id)
    {
        if (id == Guid.Empty)
            return false;

        try
        {
            var columnSet = new ColumnSet(false); // Only retrieve the ID
            await Task.Run(() => _organizationService.Retrieve(_entityLogicalName, id, columnSet));
            return true;
        }
        catch (Exception ex) when (ex.Message.Contains("does not exist"))
        {
            return false;
        }
    }
    
    /// <summary>
    /// Executes multiple operations in a single transaction.
    /// </summary>
    /// <param name="requests">The collection of organization requests to execute.</param>
    /// <returns>The responses from the executed requests.</returns>
    public async Task<ExecuteTransactionResponse> ExecuteTransactionAsync(IEnumerable<OrganizationRequest> requests)
    {
        if (requests == null)
            throw new ArgumentNullException(nameof(requests));

        var requestList = requests.ToList();
        if (!requestList.Any())
            throw new ArgumentException("At least one request is required", nameof(requests));

        var transactionRequest = new ExecuteTransactionRequest
        {
            Requests = new OrganizationRequestCollection()
        };

        foreach (var request in requestList)
        {
            transactionRequest.Requests.Add(request);
        }

        return await Task.Run(() => (ExecuteTransactionResponse)_organizationService.Execute(transactionRequest));
    }
}