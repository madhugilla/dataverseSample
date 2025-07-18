using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using DataVerseCRUDOperations.Services;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace DataVerseCRUDOperations.IntegrationTests;

/// <summary>
/// Integration tests for TransactionalService demonstrating transaction operations.
/// These tests use mocked services to demonstrate the structure for real integration tests.
/// </summary>
public class TransactionalServiceIntegrationTests : IDisposable
{
    private readonly IOrganizationService _organizationService;
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<Contact> _contactRepository;
    private readonly ITransactionalService _transactionalService;
    private readonly List<Guid> _createdAccountIds;
    private readonly List<Guid> _createdContactIds;

    public TransactionalServiceIntegrationTests()
    {
        // This would be configured with actual Dataverse connection
        // For now, we'll use a mock to demonstrate the test structure
        _organizationService = CreateMockOrganizationService();
        _accountRepository = new DataverseRepository<Account>(_organizationService);
        _contactRepository = new DataverseRepository<Contact>(_organizationService);
        _transactionalService = new TransactionalService(_accountRepository, _contactRepository);
        
        _createdAccountIds = new List<Guid>();
        _createdContactIds = new List<Guid>();
    }

    public void Dispose()
    {
        // Clean up created test data
        try
        {
            var deleteReferences = new List<(string, Guid)>();
            
            foreach (var contactId in _createdContactIds)
            {
                deleteReferences.Add(("contact", contactId));
            }
            
            foreach (var accountId in _createdAccountIds)
            {
                deleteReferences.Add(("account", accountId));
            }

            if (deleteReferences.Any())
            {
                _transactionalService.DeleteMultipleAsync(deleteReferences.ToArray()).Wait();
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_RealDataverseIntegration_ShouldCreateAndLinkEntities()
    {
        // Arrange
        var account = new Account
        {
            Name = $"Integration Test Account {DateTime.Now:yyyyMMdd_HHmmss}",
            Revenue = 1000000m,
            NumberOfEmployees = 100,
            WebSiteUrl = "https://www.integrationtest.com"
        };

        var contact1 = new Contact
        {
            FirstName = "Integration",
            LastName = "TestContact1",
            EmailAddress1 = "integration1@testcompany.com",
            JobTitle = "Manager"
        };

        var contact2 = new Contact
        {
            FirstName = "Integration",
            LastName = "TestContact2",
            EmailAddress1 = "integration2@testcompany.com",
            JobTitle = "Developer"
        };

        try
        {
            // Act
            var (accountId, contactIds) = await _transactionalService.CreateAccountWithContactsAsync(
                account, contact1, contact2);

            // Track for cleanup
            _createdAccountIds.Add(accountId);
            _createdContactIds.AddRange(contactIds);

            // Assert
            Assert.NotEqual(Guid.Empty, accountId);
            Assert.Equal(2, contactIds.Count);
            Assert.True(contactIds.All(id => id != Guid.Empty));

            // Verify account was created with correct data
            var retrievedAccount = await _accountRepository.RetrieveAsync(accountId);
            Assert.NotNull(retrievedAccount);
            Assert.Equal(account.Name, retrievedAccount.Name);
            Assert.Equal(account.Revenue, retrievedAccount.Revenue);

            // Verify contacts were created and linked
            for (int i = 0; i < contactIds.Count; i++)
            {
                var retrievedContact = await _contactRepository.RetrieveAsync(contactIds[i]);
                Assert.NotNull(retrievedContact);
                Assert.Equal(accountId, retrievedContact.ParentCustomerId);
                
                if (i == 0)
                {
                    Assert.Equal(contact1.FirstName, retrievedContact.FirstName);
                    Assert.Equal(contact1.LastName, retrievedContact.LastName);
                }
                else
                {
                    Assert.Equal(contact2.FirstName, retrievedContact.FirstName);
                    Assert.Equal(contact2.LastName, retrievedContact.LastName);
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Integration test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    public async Task CreateMultipleAsync_RealDataverseIntegration_ShouldCreateMultipleEntitiesAtomically()
    {
        // Arrange
        var account = new Account
        {
            Name = $"Bulk Test Account {DateTime.Now:yyyyMMdd_HHmmss}",
            Revenue = 2000000m
        };

        var contact = new Contact
        {
            FirstName = "Bulk",
            LastName = "TestContact",
            EmailAddress1 = "bulk@testcompany.com"
        };

        try
        {
            // Act
            var result = await _transactionalService.CreateMultipleAsync(account, contact);

            // Track for cleanup
            _createdAccountIds.Add(result[account]);
            _createdContactIds.Add(result[contact]);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.NotEqual(Guid.Empty, result[account]);
            Assert.NotEqual(Guid.Empty, result[contact]);

            // Verify entities were created
            var retrievedAccount = await _accountRepository.RetrieveAsync(result[account]);
            var retrievedContact = await _contactRepository.RetrieveAsync(result[contact]);

            Assert.NotNull(retrievedAccount);
            Assert.Equal(account.Name, retrievedAccount.Name);
            Assert.NotNull(retrievedContact);
            Assert.Equal(contact.FirstName, retrievedContact.FirstName);
        }
        catch (Exception ex)
        {
            throw new Exception($"Bulk creation integration test failed: {ex.Message}", ex);
        }
    }

    [Fact]
    public async Task UpdateMultipleAsync_RealDataverseIntegration_ShouldUpdateMultipleEntitiesAtomically()
    {
        // Arrange - First create entities to update
        var account = new Account
        {
            Name = $"Update Test Account {DateTime.Now:yyyyMMdd_HHmmss}",
            Revenue = 1000000m
        };

        var contact = new Contact
        {
            FirstName = "Update",
            LastName = "TestContact",
            EmailAddress1 = "update@testcompany.com"
        };

        try
        {
            // Create entities first
            var createResult = await _transactionalService.CreateMultipleAsync(account, contact);
            _createdAccountIds.Add(createResult[account]);
            _createdContactIds.Add(createResult[contact]);

            // Modify the entities
            account.Revenue = 3000000m;
            account.WebSiteUrl = "https://www.updated-test.com";
            contact.JobTitle = "Senior Manager";
            contact.EmailAddress1 = "updated@testcompany.com";

            // Act
            await _transactionalService.UpdateMultipleAsync(account, contact);

            // Assert
            var updatedAccount = await _accountRepository.RetrieveAsync(account.Id);
            var updatedContact = await _contactRepository.RetrieveAsync(contact.Id);

            Assert.NotNull(updatedAccount);
            Assert.Equal(3000000m, updatedAccount.Revenue);
            Assert.Equal("https://www.updated-test.com", updatedAccount.WebSiteUrl);

            Assert.NotNull(updatedContact);
            Assert.Equal("Senior Manager", updatedContact.JobTitle);
            Assert.Equal("updated@testcompany.com", updatedContact.EmailAddress1);
        }
        catch (Exception ex)
        {
            throw new Exception($"Update integration test failed: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a mock organization service for testing purposes.
    /// In a real integration test, this would be replaced with actual Dataverse connection.
    /// </summary>
    private IOrganizationService CreateMockOrganizationService()
    {
        // This is a placeholder for actual Dataverse connection
        // In real integration tests, you would configure this with:
        // - Service Principal authentication
        // - Connection string to test environment
        // - Proper error handling and retries
        
        var mock = new Mock<IOrganizationService>();
        
        // Mock Create operations
        mock.Setup(s => s.Create(It.IsAny<Entity>()))
            .Returns(() => Guid.NewGuid());

        // Mock Retrieve operations
        mock.Setup(s => s.Retrieve(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
            .Returns((string entityName, Guid id, Microsoft.Xrm.Sdk.Query.ColumnSet columns) =>
            {
                var entity = new Entity(entityName, id);
                // Add mock data based on entity type
                if (entityName == "account")
                {
                    entity["name"] = "Mock Account";
                    entity["revenue"] = new Money(1000000m);
                }
                else if (entityName == "contact")
                {
                    entity["firstname"] = "Mock";
                    entity["lastname"] = "Contact";
                    entity["parentcustomerid"] = new EntityReference("account", Guid.NewGuid());
                }
                return entity;
            });

        // Mock Update operations
        mock.Setup(s => s.Update(It.IsAny<Entity>()));

        // Mock Delete operations
        mock.Setup(s => s.Delete(It.IsAny<string>(), It.IsAny<Guid>()));

        // Mock Execute operations for transactions
        mock.Setup(s => s.Execute(It.IsAny<OrganizationRequest>()))
            .Returns((OrganizationRequest request) =>
            {
                if (request is Microsoft.Xrm.Sdk.Messages.ExecuteTransactionRequest transactionRequest)
                {
                    var response = new Microsoft.Xrm.Sdk.Messages.ExecuteTransactionResponse();
                    var responses = new OrganizationResponseCollection();
                    
                    foreach (var req in transactionRequest.Requests)
                    {
                        if (req is Microsoft.Xrm.Sdk.Messages.CreateRequest)
                        {
                            responses.Add(new Microsoft.Xrm.Sdk.Messages.CreateResponse 
                            { 
                                Results = { ["id"] = Guid.NewGuid() } 
                            });
                        }
                        else if (req is Microsoft.Xrm.Sdk.Messages.UpdateRequest)
                        {
                            responses.Add(new Microsoft.Xrm.Sdk.Messages.UpdateResponse());
                        }
                        else if (req is Microsoft.Xrm.Sdk.Messages.DeleteRequest)
                        {
                            responses.Add(new Microsoft.Xrm.Sdk.Messages.DeleteResponse());
                        }
                    }
                    
                    response.Results["Responses"] = responses;
                    return response;
                }
                
                return new OrganizationResponse();
            });

        return mock.Object;
    }
}