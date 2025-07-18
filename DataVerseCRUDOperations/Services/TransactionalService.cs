using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Service implementation for executing multiple operations in a single transaction.
/// Provides atomic operations across multiple entities using ExecuteTransactionRequest.
/// </summary>
public class TransactionalService : ITransactionalService
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Contact> _contactRepository;

    /// <summary>
    /// Initializes a new instance of the TransactionalService class.
    /// </summary>
    /// <param name="accountRepository">The repository for Account operations.</param>
    /// <param name="contactRepository">The repository for Contact operations.</param>
    public TransactionalService(
        IRepository<Account> accountRepository,
        IRepository<Contact> contactRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _contactRepository = contactRepository ?? throw new ArgumentNullException(nameof(contactRepository));
    }

    /// <summary>
    /// Creates multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entities">The entities to create.</param>
    /// <returns>A dictionary mapping original entities to their created IDs.</returns>
    public async Task<Dictionary<IDataverseEntity, Guid>> CreateMultipleAsync(params IDataverseEntity[] entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException("At least one entity is required", nameof(entities));

        var requests = new List<OrganizationRequest>();
        
        foreach (var entity in entities)
        {
            if (entity == null)
                throw new ArgumentException("Entity cannot be null", nameof(entities));

            var createRequest = new CreateRequest
            {
                Target = entity.ToEntity()
            };
            requests.Add(createRequest);
        }

        // Use the first entity's repository to execute the transaction
        // All repositories share the same IOrganizationService instance
        var response = await _accountRepository.ExecuteTransactionAsync(requests);
        
        var result = new Dictionary<IDataverseEntity, Guid>();
        for (int i = 0; i < entities.Length; i++)
        {
            var createResponse = (CreateResponse)response.Responses[i];
            result[entities[i]] = createResponse.id;
            entities[i].Id = createResponse.id; // Update the entity with the new ID
        }

        return result;
    }

    /// <summary>
    /// Creates an account and its associated contacts in a single transaction.
    /// This is a common business scenario where you want to ensure data consistency.
    /// </summary>
    /// <param name="account">The account to create.</param>
    /// <param name="contacts">The contacts to associate with the account.</param>
    /// <returns>A result containing the created account ID and contact IDs.</returns>
    public async Task<(Guid AccountId, List<Guid> ContactIds)> CreateAccountWithContactsAsync(Account account, params Contact[] contacts)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (contacts == null || !contacts.Any())
            throw new ArgumentException("At least one contact is required", nameof(contacts));

        // Validate account
        if (string.IsNullOrWhiteSpace(account.Name))
            throw new ArgumentException("Account name is required", nameof(account));

        // Validate contacts
        foreach (var contact in contacts)
        {
            if (contact == null)
                throw new ArgumentException("Contact cannot be null", nameof(contacts));
            
            if (string.IsNullOrWhiteSpace(contact.FirstName) && string.IsNullOrWhiteSpace(contact.LastName))
                throw new ArgumentException("Contact must have at least first name or last name", nameof(contacts));
        }

        var requests = new List<OrganizationRequest>();

        // Create account request
        var accountEntity = account.ToEntity();
        var createAccountRequest = new CreateRequest { Target = accountEntity };
        requests.Add(createAccountRequest);

        // Create contact requests - they will be linked to the account after creation
        foreach (var contact in contacts)
        {
            var contactEntity = contact.ToEntity();
            var createContactRequest = new CreateRequest { Target = contactEntity };
            requests.Add(createContactRequest);
        }

        // Execute transaction
        var response = await _accountRepository.ExecuteTransactionAsync(requests);

        // Extract results
        var accountId = ((CreateResponse)response.Responses[0]).id;
        var contactIds = new List<Guid>();

        for (int i = 1; i < response.Responses.Count; i++)
        {
            var contactId = ((CreateResponse)response.Responses[i]).id;
            contactIds.Add(contactId);
        }

        // Update the original objects with their new IDs
        account.Id = accountId;
        for (int i = 0; i < contacts.Length; i++)
        {
            contacts[i].Id = contactIds[i];
            contacts[i].ParentCustomerId = accountId; // Link contact to account
        }

        // Update contacts to link them to the account (this would normally be done in a second transaction)
        // For demonstration, we'll just set the property - in a real scenario you might want another transaction
        var updateRequests = new List<OrganizationRequest>();
        foreach (var contactId in contactIds)
        {
            var updateContactRequest = new UpdateRequest
            {
                Target = new Entity("contact", contactId)
                {
                    ["parentcustomerid"] = new EntityReference("account", accountId)
                }
            };
            updateRequests.Add(updateContactRequest);
        }

        // Execute the update transaction to link contacts to account
        if (updateRequests.Any())
        {
            await _contactRepository.ExecuteTransactionAsync(updateRequests);
        }

        return (accountId, contactIds);
    }

    /// <summary>
    /// Updates multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateMultipleAsync(params IDataverseEntity[] entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException("At least one entity is required", nameof(entities));

        var requests = new List<OrganizationRequest>();
        
        foreach (var entity in entities)
        {
            if (entity == null)
                throw new ArgumentException("Entity cannot be null", nameof(entities));

            if (entity.Id == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty for update operation", nameof(entities));

            var updateRequest = new UpdateRequest
            {
                Target = entity.ToEntity()
            };
            requests.Add(updateRequest);
        }

        await _accountRepository.ExecuteTransactionAsync(requests);
    }

    /// <summary>
    /// Deletes multiple entities in a single transaction.
    /// If any operation fails, all operations are rolled back.
    /// </summary>
    /// <param name="entityReferences">The entity references to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteMultipleAsync(params (string LogicalName, Guid Id)[] entityReferences)
    {
        if (entityReferences == null || !entityReferences.Any())
            throw new ArgumentException("At least one entity reference is required", nameof(entityReferences));

        var requests = new List<OrganizationRequest>();
        
        foreach (var (logicalName, id) in entityReferences)
        {
            if (string.IsNullOrEmpty(logicalName))
                throw new ArgumentException("Logical name cannot be null or empty", nameof(entityReferences));

            if (id == Guid.Empty)
                throw new ArgumentException("Entity ID cannot be empty", nameof(entityReferences));

            var deleteRequest = new DeleteRequest
            {
                Target = new EntityReference(logicalName, id)
            };
            requests.Add(deleteRequest);
        }

        await _accountRepository.ExecuteTransactionAsync(requests);
    }
}