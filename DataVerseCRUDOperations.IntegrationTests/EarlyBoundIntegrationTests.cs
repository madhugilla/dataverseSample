using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using DataVerseCRUDOperations.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace DataVerseCRUDOperations.IntegrationTests;

/// <summary>
/// Integration tests for the early bound implementation.
/// These tests require a real Dataverse connection and will be skipped if configuration is not available.
/// </summary>
public class EarlyBoundIntegrationTests : IDisposable
{
    private readonly IOrganizationService? _organizationService;
    private readonly AccountService? _accountService;
    private readonly ContactService? _contactService;
    private readonly List<Guid> _createdAccountIds = new();
    private readonly List<Guid> _createdContactIds = new();
    private readonly bool _canRunTests;

    public EarlyBoundIntegrationTests()
    {
        try
        {
            // Try to get configuration - if it fails, tests will be skipped
            var config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var url = config["Dataverse:Url"];
            var clientId = config["Dataverse:ClientId"];
            var clientSecret = config["Dataverse:ClientSecret"];

            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                string connectionString = $@"
                    AuthType=ClientSecret;
                    Url={url};
                    ClientId={clientId};
                    ClientSecret={clientSecret};
                ";

                var serviceClient = new ServiceClient(connectionString);
                if (serviceClient.IsReady)
                {
                    _organizationService = serviceClient;
                    
                    var accountRepository = new DataverseRepository<Account>(_organizationService);
                    var contactRepository = new DataverseRepository<Contact>(_organizationService);
                    
                    _accountService = new AccountService(accountRepository);
                    _contactService = new ContactService(contactRepository);
                    
                    _canRunTests = true;
                }
            }
        }
        catch
        {
            // Configuration not available - tests will be skipped
            _canRunTests = false;
        }
    }

    [Fact]
    public async Task AccountService_FullCrudOperations_ShouldWorkCorrectly()
    {
        // Skip test if no configuration
        if (!_canRunTests || _accountService == null)
        {
            // This test is skipped due to missing Dataverse configuration
            return;
        }

        // Create
        var newAccount = new Account
        {
            Name = $"Integration Test Account {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            AccountNumber = $"IT-{DateTime.Now:yyyyMMddHHmmss}",
            Description = "Account created by integration test",
            Revenue = 750000m,
            NumberOfEmployees = 50
        };

        var createdAccount = await _accountService.CreateAsync(newAccount);
        _createdAccountIds.Add(createdAccount.Id);

        Assert.NotEqual(Guid.Empty, createdAccount.Id);
        Assert.Equal(newAccount.Name, createdAccount.Name);

        // Read
        var retrievedAccount = await _accountService.GetByIdAsync(createdAccount.Id);
        Assert.NotNull(retrievedAccount);
        Assert.Equal(createdAccount.Name, retrievedAccount.Name);
        Assert.Equal(createdAccount.Revenue, retrievedAccount.Revenue);

        // Update
        retrievedAccount!.Name = $"Updated {retrievedAccount.Name}";
        retrievedAccount.Revenue = 900000m;
        
        var updatedAccount = await _accountService.UpdateAsync(retrievedAccount);
        Assert.Equal(retrievedAccount.Name, updatedAccount.Name);
        Assert.Equal(900000m, updatedAccount.Revenue);

        // Search
        var searchResults = await _accountService.SearchByNameAsync("Integration Test", exactMatch: false);
        Assert.Contains(searchResults, a => a.Id == createdAccount.Id);

        // High revenue search
        var highRevenueAccounts = await _accountService.GetHighRevenueAccountsAsync(800000m);
        Assert.Contains(highRevenueAccounts, a => a.Id == createdAccount.Id);

        // Exists check
        var exists = await _accountService.ExistsAsync(createdAccount.Id);
        Assert.True(exists);

        // Delete
        var deleted = await _accountService.DeleteAsync(createdAccount.Id);
        Assert.True(deleted);

        // Verify deletion
        var existsAfterDelete = await _accountService.ExistsAsync(createdAccount.Id);
        Assert.False(existsAfterDelete);

        _createdAccountIds.Remove(createdAccount.Id); // Remove from cleanup list since it's already deleted
    }

    [Fact]
    public async Task ContactService_WithAccountRelationship_ShouldWorkCorrectly()
    {
        // Skip test if no configuration
        if (!_canRunTests || _accountService == null || _contactService == null)
        {
            return;
        }

        // First create an account
        var account = new Account
        {
            Name = $"Contact Test Account {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            AccountNumber = $"CTA-{DateTime.Now:yyyyMMddHHmmss}"
        };

        var createdAccount = await _accountService.CreateAsync(account);
        _createdAccountIds.Add(createdAccount.Id);

        // Create a contact associated with the account
        var contact = new Contact
        {
            FirstName = "Integration",
            LastName = "Test Contact",
            EmailAddress1 = $"integration.test.{DateTime.Now:yyyyMMddHHmmss}@example.com",
            JobTitle = "Test Engineer",
            ParentCustomerId = createdAccount.Id
        };

        var createdContact = await _contactService.CreateAsync(contact);
        _createdContactIds.Add(createdContact.Id);

        Assert.NotEqual(Guid.Empty, createdContact.Id);
        Assert.Equal("Integration", createdContact.FirstName);
        Assert.Equal("Test Contact", createdContact.LastName);

        // Test contact search by email
        var contactByEmail = await _contactService.GetByEmailAsync(contact.EmailAddress1!);
        Assert.NotNull(contactByEmail);
        Assert.Equal(createdContact.Id, contactByEmail.Id);

        // Test getting contacts by account
        var accountContacts = await _contactService.GetContactsByAccountAsync(createdAccount.Id);
        Assert.Contains(accountContacts, c => c.Id == createdContact.Id);

        // Test search by name
        var nameSearchResults = await _contactService.SearchByNameAsync("Integration");
        Assert.Contains(nameSearchResults, c => c.Id == createdContact.Id);

        // Clean up - delete contact first, then account
        await _contactService.DeleteAsync(createdContact.Id);
        await _accountService.DeleteAsync(createdAccount.Id);

        // Remove from cleanup lists
        _createdContactIds.Remove(createdContact.Id);
        _createdAccountIds.Remove(createdAccount.Id);
    }

    [Fact]
    public async Task DataverseRepository_DirectOperations_ShouldWorkCorrectly()
    {
        // Skip test if no configuration
        if (!_canRunTests || _organizationService == null)
        {
            return;
        }

        var repository = new DataverseRepository<Account>(_organizationService);

        // Create an account through repository
        var account = new Account
        {
            Name = $"Repository Test {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Description = "Testing repository directly"
        };

        var accountId = await repository.CreateAsync(account);
        _createdAccountIds.Add(accountId);

        Assert.NotEqual(Guid.Empty, accountId);

        // Retrieve the account
        var retrievedAccount = await repository.RetrieveAsync(accountId);
        Assert.NotNull(retrievedAccount);
        Assert.Equal(account.Name, retrievedAccount.Name);

        // Update the account
        retrievedAccount!.Description = "Updated through repository";
        await repository.UpdateAsync(retrievedAccount);

        // Verify update
        var updatedAccount = await repository.RetrieveAsync(accountId);
        Assert.Equal("Updated through repository", updatedAccount!.Description);

        // Test exists
        var exists = await repository.ExistsAsync(accountId);
        Assert.True(exists);

        // Clean up
        await repository.DeleteAsync(accountId);
        _createdAccountIds.Remove(accountId);

        // Verify deletion
        var existsAfterDelete = await repository.ExistsAsync(accountId);
        Assert.False(existsAfterDelete);
    }

    public void Dispose()
    {
        // Clean up any remaining test data
        if (_canRunTests && _organizationService != null)
        {
            try
            {
                // Delete any remaining contacts
                foreach (var contactId in _createdContactIds)
                {
                    try
                    {
                        _organizationService.Delete("contact", contactId);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }

                // Delete any remaining accounts
                foreach (var accountId in _createdAccountIds)
                {
                    try
                    {
                        _organizationService.Delete("account", accountId);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        if (_organizationService is IDisposable disposableService)
        {
            disposableService.Dispose();
        }
    }
}