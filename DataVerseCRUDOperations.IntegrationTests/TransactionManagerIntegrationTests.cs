using DataVerseCRUDOperations.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Xunit;

namespace DataVerseCRUDOperations.IntegrationTests;

/// <summary>
/// Integration tests for TransactionManager demonstrating transaction operations.
/// These tests use mocked services to demonstrate the structure for real integration tests.
/// </summary>
public class TransactionManagerIntegrationTests : IDisposable
{
    private readonly IOrganizationService _organizationService;
    private readonly ITransactionManager _transactionManager;
    private readonly List<Guid> _createdAccountIds;
    private readonly List<Guid> _createdContactIds;

    public TransactionManagerIntegrationTests()
    {
        // This would be configured with actual Dataverse connection
        // For now, we'll use a mock to demonstrate the test structure
        _organizationService = CreateMockOrganizationService();
        _transactionManager = new TransactionManager(_organizationService);
        
        _createdAccountIds = new List<Guid>();
        _createdContactIds = new List<Guid>();
    }

    public void Dispose()
    {
        // Clean up created test data
        try
        {
            var deleteReferences = new List<EntityReference>();
            
            foreach (var contactId in _createdContactIds)
            {
                deleteReferences.Add(new EntityReference("contact", contactId));
            }
            
            foreach (var accountId in _createdAccountIds)
            {
                deleteReferences.Add(new EntityReference("account", accountId));
            }

            if (deleteReferences.Any())
            {
                _transactionManager.DeleteMultipleAsync(deleteReferences).Wait();
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }

    [Fact]
    public async Task CreateMultipleAsync_WithAccountAndContacts_ShouldCreateAllEntitiesAtomically()
    {
        // Arrange
        var account = new Entity("account")
        {
            ["name"] = "Integration Test Account",
            ["revenue"] = new Money(1000000),
            ["numberofemployees"] = 100
        };

        var contact1 = new Entity("contact")
        {
            ["firstname"] = "John",
            ["lastname"] = "Doe",
            ["emailaddress1"] = "john.doe@integration.test"
        };

        var contact2 = new Entity("contact")
        {
            ["firstname"] = "Jane",
            ["lastname"] = "Smith",
            ["emailaddress1"] = "jane.smith@integration.test"
        };

        var entities = new List<Entity> { account, contact1, contact2 };

        // Act
        var response = await _transactionManager.CreateMultipleAsync(entities);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Responses.Count);

        // Track created entities for cleanup
        for (int i = 0; i < response.Responses.Count; i++)
        {
            var createResponse = (CreateResponse)response.Responses[i];
            if (i == 0) // Account
            {
                _createdAccountIds.Add(createResponse.id);
            }
            else // Contacts
            {
                _createdContactIds.Add(createResponse.id);
            }
        }
    }

    [Fact]
    public async Task UpdateMultipleAsync_WithExistingEntities_ShouldUpdateAllEntitiesAtomically()
    {
        // Arrange - First create entities
        var account = new Entity("account")
        {
            ["name"] = "Update Test Account"
        };

        var contact = new Entity("contact")
        {
            ["firstname"] = "Update",
            ["lastname"] = "Test"
        };

        var createResponse = await _transactionManager.CreateMultipleAsync(new List<Entity> { account, contact });
        var accountId = ((CreateResponse)createResponse.Responses[0]).id;
        var contactId = ((CreateResponse)createResponse.Responses[1]).id;

        _createdAccountIds.Add(accountId);
        _createdContactIds.Add(contactId);

        // Prepare updates
        var updatedAccount = new Entity("account", accountId)
        {
            ["name"] = "Updated Account Name",
            ["revenue"] = new Money(2000000)
        };

        var updatedContact = new Entity("contact", contactId)
        {
            ["firstname"] = "Updated",
            ["lastname"] = "Name"
        };

        var entitiesToUpdate = new List<Entity> { updatedAccount, updatedContact };

        // Act
        var updateResponse = await _transactionManager.UpdateMultipleAsync(entitiesToUpdate);

        // Assert
        Assert.NotNull(updateResponse);
        Assert.Equal(2, updateResponse.Responses.Count);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithMixedOperations_ShouldExecuteAtomically()
    {
        // Arrange - First create an entity to update/delete
        var initialAccount = new Entity("account")
        {
            ["name"] = "Mixed Operations Test Account"
        };

        var createResponse = await _transactionManager.CreateMultipleAsync(new List<Entity> { initialAccount });
        var existingAccountId = ((CreateResponse)createResponse.Responses[0]).id;
        _createdAccountIds.Add(existingAccountId);

        // Prepare mixed operations
        var requests = new List<OrganizationRequest>();

        // 1. Create a new account
        requests.Add(new CreateRequest
        {
            Target = new Entity("account")
            {
                ["name"] = "New Account from Mixed Operations"
            }
        });

        // 2. Update the existing account
        requests.Add(new UpdateRequest
        {
            Target = new Entity("account", existingAccountId)
            {
                ["name"] = "Updated Account from Mixed Operations"
            }
        });

        // 3. Create a contact
        requests.Add(new CreateRequest
        {
            Target = new Entity("contact")
            {
                ["firstname"] = "Mixed",
                ["lastname"] = "Operations"
            }
        });

        // Act
        var response = await _transactionManager.ExecuteTransactionAsync(requests);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(3, response.Responses.Count);

        // Track the newly created entities for cleanup
        _createdAccountIds.Add(((CreateResponse)response.Responses[0]).id);
        _createdContactIds.Add(((CreateResponse)response.Responses[2]).id);
    }

    private static IOrganizationService CreateMockOrganizationService()
    {
        var mock = new Mock<IOrganizationService>();
        
        // Mock ExecuteTransactionRequest
        mock.Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Returns<OrganizationRequest>(request =>
            {
                var transactionRequest = (ExecuteTransactionRequest)request;
                var responses = new OrganizationResponseCollection();

                foreach (var req in transactionRequest.Requests)
                {
                    if (req is CreateRequest)
                    {
                        responses.Add(new CreateResponse
                        {
                            Results = { ["id"] = Guid.NewGuid() }
                        });
                    }
                    else if (req is UpdateRequest)
                    {
                        responses.Add(new UpdateResponse());
                    }
                    else if (req is DeleteRequest)
                    {
                        responses.Add(new DeleteResponse());
                    }
                }

                return new ExecuteTransactionResponse
                {
                    Results = { ["Responses"] = responses }
                };
            });

        return mock.Object;
    }
}