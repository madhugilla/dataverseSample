using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Service interface specific to Contact entity operations.
/// Provides Contact-specific business logic.
/// </summary>
public interface IContactService : IDataverseService<Contact>
{
    /// <summary>
    /// Searches for contacts by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The contact with the specified email, or null if not found.</returns>
    Task<Contact?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets all contacts associated with a specific account.
    /// </summary>
    /// <param name="accountId">The ID of the parent account.</param>
    /// <returns>A collection of contacts associated with the account.</returns>
    Task<IEnumerable<Contact>> GetContactsByAccountAsync(Guid accountId);
    
    /// <summary>
    /// Searches for contacts by name (first name or last name).
    /// </summary>
    /// <param name="searchTerm">The name to search for.</param>
    /// <returns>A collection of matching contacts.</returns>
    Task<IEnumerable<Contact>> SearchByNameAsync(string searchTerm);
}

/// <summary>
/// Service implementation specific to Contact entity operations.
/// Provides Contact-specific business logic.
/// </summary>
public class ContactService : DataverseService<Contact>, IContactService
{
    private readonly IRepository<Contact> _repository;

    /// <summary>
    /// Initializes a new instance of the ContactService class.
    /// </summary>
    /// <param name="repository">The repository for Contact data access.</param>
    public ContactService(IRepository<Contact> repository) : base(repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Searches for contacts by email address.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>The contact with the specified email, or null if not found.</returns>
    public async Task<Contact?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        var results = await SearchAsync("emailaddress1", email, ConditionOperator.Equal);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Gets all contacts associated with a specific account.
    /// </summary>
    /// <param name="accountId">The ID of the parent account.</param>
    /// <returns>A collection of contacts associated with the account.</returns>
    public async Task<IEnumerable<Contact>> GetContactsByAccountAsync(Guid accountId)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty", nameof(accountId));

        return await SearchAsync("parentcustomerid", accountId, ConditionOperator.Equal, 
            "firstname", "lastname", "emailaddress1", "telephone1", "jobtitle");
    }

    /// <summary>
    /// Searches for contacts by name (first name or last name).
    /// </summary>
    /// <param name="searchTerm">The name to search for.</param>
    /// <returns>A collection of matching contacts.</returns>
    public async Task<IEnumerable<Contact>> SearchByNameAsync(string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            throw new ArgumentException("Search term cannot be null or empty", nameof(searchTerm));

        var query = new QueryExpression("contact")
        {
            ColumnSet = new ColumnSet("firstname", "lastname", "fullname", "emailaddress1", "telephone1")
        };

        // Create filter for first name OR last name
        var filter = new FilterExpression(LogicalOperator.Or);
        filter.AddCondition("firstname", ConditionOperator.BeginsWith, searchTerm);
        filter.AddCondition("lastname", ConditionOperator.BeginsWith, searchTerm);
        
        query.Criteria.AddFilter(filter);
        query.Orders.Add(new OrderExpression("lastname", OrderType.Ascending));
        query.Orders.Add(new OrderExpression("firstname", OrderType.Ascending));

        return await _repository.RetrieveMultipleAsync(query);
    }

    /// <summary>
    /// Validates the Contact entity before create or update operations.
    /// </summary>
    /// <param name="entity">The Contact entity to validate.</param>
    protected override void ValidateEntity(Contact entity)
    {
        base.ValidateEntity(entity);

        if (string.IsNullOrWhiteSpace(entity.LastName))
        {
            throw new ArgumentException("Contact last name is required", nameof(entity));
        }

        if (entity.LastName.Length > 50)
        {
            throw new ArgumentException("Last name cannot exceed 50 characters", nameof(entity));
        }

        if (!string.IsNullOrEmpty(entity.FirstName) && entity.FirstName.Length > 50)
        {
            throw new ArgumentException("First name cannot exceed 50 characters", nameof(entity));
        }

        if (!string.IsNullOrEmpty(entity.EmailAddress1) && !IsValidEmail(entity.EmailAddress1))
        {
            throw new ArgumentException("Invalid email address format", nameof(entity));
        }
    }

    /// <summary>
    /// Validates email address format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email format is valid, false otherwise.</returns>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}