using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using Microsoft.Xrm.Sdk.Query;

namespace DataVerseCRUDOperations.Services;

/// <summary>
/// Service interface specific to Account entity operations.
/// Provides Account-specific business logic.
/// </summary>
public interface IAccountService : IDataverseService<Account>
{
    /// <summary>
    /// Searches for accounts by name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="exactMatch">Whether to search for exact match or partial match.</param>
    /// <returns>A collection of matching accounts.</returns>
    Task<IEnumerable<Account>> SearchByNameAsync(string name, bool exactMatch = false);
    
    /// <summary>
    /// Gets accounts with revenue greater than the specified amount.
    /// </summary>
    /// <param name="minRevenue">The minimum revenue threshold.</param>
    /// <returns>A collection of accounts with revenue above the threshold.</returns>
    Task<IEnumerable<Account>> GetHighRevenueAccountsAsync(decimal minRevenue);
}

/// <summary>
/// Service implementation specific to Account entity operations.
/// Provides Account-specific business logic.
/// </summary>
public class AccountService : DataverseService<Account>, IAccountService
{
    private readonly IRepository<Account> _repository;

    /// <summary>
    /// Initializes a new instance of the AccountService class.
    /// </summary>
    /// <param name="repository">The repository for Account data access.</param>
    public AccountService(IRepository<Account> repository) : base(repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Searches for accounts by name.
    /// </summary>
    /// <param name="name">The name to search for.</param>
    /// <param name="exactMatch">Whether to search for exact match or partial match.</param>
    /// <returns>A collection of matching accounts.</returns>
    public async Task<IEnumerable<Account>> SearchByNameAsync(string name, bool exactMatch = false)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        var conditionOperator = exactMatch ? ConditionOperator.Equal : ConditionOperator.BeginsWith;
        return await SearchAsync("name", name, conditionOperator, "name", "accountnumber", "telephone1", "websiteurl");
    }

    /// <summary>
    /// Gets accounts with revenue greater than the specified amount.
    /// </summary>
    /// <param name="minRevenue">The minimum revenue threshold.</param>
    /// <returns>A collection of accounts with revenue above the threshold.</returns>
    public async Task<IEnumerable<Account>> GetHighRevenueAccountsAsync(decimal minRevenue)
    {
        var query = new QueryExpression("account")
        {
            ColumnSet = new ColumnSet("name", "revenue", "numberofemployees")
        };
        
        query.Criteria.AddCondition("revenue", ConditionOperator.GreaterThan, minRevenue);
        query.Orders.Add(new OrderExpression("revenue", OrderType.Descending));

        return await _repository.RetrieveMultipleAsync(query);
    }

    /// <summary>
    /// Validates the Account entity before create or update operations.
    /// </summary>
    /// <param name="entity">The Account entity to validate.</param>
    protected override void ValidateEntity(Account entity)
    {
        base.ValidateEntity(entity);

        if (string.IsNullOrWhiteSpace(entity.Name))
        {
            throw new ArgumentException("Account name is required", nameof(entity));
        }

        if (entity.Name.Length > 160)
        {
            throw new ArgumentException("Account name cannot exceed 160 characters", nameof(entity));
        }

        if (entity.Revenue.HasValue && entity.Revenue < 0)
        {
            throw new ArgumentException("Revenue cannot be negative", nameof(entity));
        }

        if (entity.NumberOfEmployees.HasValue && entity.NumberOfEmployees < 0)
        {
            throw new ArgumentException("Number of employees cannot be negative", nameof(entity));
        }
    }
}