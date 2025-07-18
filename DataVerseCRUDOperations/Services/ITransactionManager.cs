using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Generic transaction manager interface for executing multiple Dataverse operations atomically.
/// Provides a simple, extensible approach without entity-specific dependencies.
/// </summary>
public interface ITransactionManager
{
    /// <summary>
    /// Executes multiple requests in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="requests">The collection of organization requests to execute.</param>
    /// <returns>The responses from the executed requests.</returns>
    Task<ExecuteTransactionResponse> ExecuteTransactionAsync(IEnumerable<OrganizationRequest> requests);
    
    /// <summary>
    /// Creates multiple entities in a single transaction.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <returns>The responses containing the created entity IDs.</returns>
    Task<ExecuteTransactionResponse> CreateMultipleAsync(IEnumerable<Entity> entities);
    
    /// <summary>
    /// Updates multiple entities in a single transaction.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>The transaction response.</returns>
    Task<ExecuteTransactionResponse> UpdateMultipleAsync(IEnumerable<Entity> entities);
    
    /// <summary>
    /// Deletes multiple entities in a single transaction.
    /// </summary>
    /// <param name="entityReferences">The entity references to delete.</param>
    /// <returns>The transaction response.</returns>
    Task<ExecuteTransactionResponse> DeleteMultipleAsync(IEnumerable<EntityReference> entityReferences);
}