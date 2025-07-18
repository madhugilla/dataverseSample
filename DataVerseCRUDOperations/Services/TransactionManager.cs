using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Generic transaction manager implementation for executing multiple Dataverse operations atomically.
/// Uses IOrganizationService directly, making it simple and extensible without entity-specific dependencies.
/// </summary>
public class TransactionManager : ITransactionManager
{
    private readonly IOrganizationService _organizationService;

    /// <summary>
    /// Initializes a new instance of the TransactionManager class.
    /// </summary>
    /// <param name="organizationService">The organization service for Dataverse operations.</param>
    public TransactionManager(IOrganizationService organizationService)
    {
        _organizationService = organizationService ?? throw new ArgumentNullException(nameof(organizationService));
    }

    /// <summary>
    /// Executes multiple requests in a single transaction.
    /// If any operation fails, all operations are rolled back.
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
            if (request == null)
                throw new ArgumentException("Request cannot be null", nameof(requests));
            
            transactionRequest.Requests.Add(request);
        }

        return await Task.Run(() => (ExecuteTransactionResponse)_organizationService.Execute(transactionRequest));
    }

    /// <summary>
    /// Creates multiple entities in a single transaction.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <returns>The responses containing the created entity IDs.</returns>
    public async Task<ExecuteTransactionResponse> CreateMultipleAsync(IEnumerable<Entity> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("At least one entity is required", nameof(entities));

        var requests = entityList.Select(entity =>
        {
            if (entity == null)
                throw new ArgumentException("Entity cannot be null", nameof(entities));

            return new CreateRequest { Target = entity };
        }).Cast<OrganizationRequest>();

        return await ExecuteTransactionAsync(requests);
    }

    /// <summary>
    /// Updates multiple entities in a single transaction.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>The transaction response.</returns>
    public async Task<ExecuteTransactionResponse> UpdateMultipleAsync(IEnumerable<Entity> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var entityList = entities.ToList();
        if (!entityList.Any())
            throw new ArgumentException("At least one entity is required", nameof(entities));

        var requests = entityList.Select(entity =>
        {
            if (entity == null)
                throw new ArgumentException("Entity cannot be null", nameof(entities));

            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty for update operation", nameof(entities));

            return new UpdateRequest { Target = entity };
        }).Cast<OrganizationRequest>();

        return await ExecuteTransactionAsync(requests);
    }

    /// <summary>
    /// Deletes multiple entities in a single transaction.
    /// </summary>
    /// <param name="entityReferences">The entity references to delete.</param>
    /// <returns>The transaction response.</returns>
    public async Task<ExecuteTransactionResponse> DeleteMultipleAsync(IEnumerable<EntityReference> entityReferences)
    {
        if (entityReferences == null)
            throw new ArgumentNullException(nameof(entityReferences));

        var referenceList = entityReferences.ToList();
        if (!referenceList.Any())
            throw new ArgumentException("At least one entity reference is required", nameof(entityReferences));

        var requests = referenceList.Select(entityRef =>
        {
            if (entityRef == null)
                throw new ArgumentException("Entity reference cannot be null", nameof(entityReferences));

            if (string.IsNullOrEmpty(entityRef.LogicalName))
                throw new ArgumentException("Entity logical name cannot be null or empty", nameof(entityReferences));

            if (entityRef.Id == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty", nameof(entityReferences));

            return new DeleteRequest { Target = entityRef };
        }).Cast<OrganizationRequest>();

        return await ExecuteTransactionAsync(requests);
    }
}