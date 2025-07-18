using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Service interface for executing multiple operations in a single transaction.
/// Provides atomic operations across multiple entities.
/// </summary>
public interface ITransactionalService
{
    /// <summary>
    /// Creates multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <returns>A dictionary mapping original entities to their created IDs.</returns>
    Task<Dictionary<IDataverseEntity, Guid>> CreateMultipleAsync(params IDataverseEntity[] entities);
    
    /// <summary>
    /// Creates an account and its associated contacts in a single transaction.
    /// This is a common business scenario where you want to ensure data consistency.
    /// </summary>
    /// <param name="account">The account to create.</param>
    /// <param name="contacts">The contacts to associate with the account.</param>
    /// <returns>A result containing the created account ID and contact IDs.</returns>
    Task<(Guid AccountId, List<Guid> ContactIds)> CreateAccountWithContactsAsync(Account account, params Contact[] contacts);
    
    /// <summary>
    /// Updates multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateMultipleAsync(params IDataverseEntity[] entities);
    
    /// <summary>
    /// Deletes multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entityReferences">The entity references to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteMultipleAsync(params (string LogicalName, Guid Id)[] entityReferences);
}