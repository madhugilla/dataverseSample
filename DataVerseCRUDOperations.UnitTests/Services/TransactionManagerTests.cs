using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Xunit;
using DataVerseCRUDOperations.Services;

namespace DataVerseCRUDOperations.UnitTests.Services;

/// <summary>
/// Unit tests for the TransactionManager class.
/// </summary>
public class TransactionManagerTests
{
    private readonly Mock<IOrganizationService> _mockOrganizationService;
    private readonly TransactionManager _transactionManager;

    public TransactionManagerTests()
    {
        _mockOrganizationService = new Mock<IOrganizationService>();
        _transactionManager = new TransactionManager(_mockOrganizationService.Object);
    }

    [Fact]
    public void Constructor_WithNullOrganizationService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TransactionManager(null!));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithValidRequests_ShouldExecuteSuccessfully()
    {
        // Arrange
        var createRequest = new CreateRequest
        {
            Target = new Entity("account")
            {
                ["name"] = "Test Account"
            }
        };

        var requests = new List<OrganizationRequest> { createRequest };
        var expectedResponse = new ExecuteTransactionResponse();
        expectedResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new CreateResponse { Results = { ["id"] = Guid.NewGuid() } }
        };

        _mockOrganizationService
            .Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Returns(expectedResponse);

        // Act
        var result = await _transactionManager.ExecuteTransactionAsync(requests);

        // Assert
        Assert.NotNull(result);
        _mockOrganizationService.Verify(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithNullRequests_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _transactionManager.ExecuteTransactionAsync(null!));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithEmptyRequests_ShouldThrowArgumentException()
    {
        // Arrange
        var requests = new List<OrganizationRequest>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.ExecuteTransactionAsync(requests));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WithNullRequestInList_ShouldThrowArgumentException()
    {
        // Arrange
        var requests = new List<OrganizationRequest> { null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.ExecuteTransactionAsync(requests));
    }

    [Fact]
    public async Task CreateMultipleAsync_WithValidEntities_ShouldCreateSuccessfully()
    {
        // Arrange
        var account = new Entity("account") { ["name"] = "Test Account" };
        var contact = new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" };
        var entities = new List<Entity> { account, contact };

        var expectedResponse = new ExecuteTransactionResponse();
        expectedResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new CreateResponse { Results = { ["id"] = Guid.NewGuid() } },
            new CreateResponse { Results = { ["id"] = Guid.NewGuid() } }
        };

        _mockOrganizationService
            .Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Returns(expectedResponse);

        // Act
        var result = await _transactionManager.CreateMultipleAsync(entities);

        // Assert
        Assert.NotNull(result);
        _mockOrganizationService.Verify(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()), Times.Once);
    }

    [Fact]
    public async Task CreateMultipleAsync_WithNullEntities_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _transactionManager.CreateMultipleAsync(null!));
    }

    [Fact]
    public async Task CreateMultipleAsync_WithEmptyEntities_ShouldThrowArgumentException()
    {
        // Arrange
        var entities = new List<Entity>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.CreateMultipleAsync(entities));
    }

    [Fact]
    public async Task CreateMultipleAsync_WithNullEntityInList_ShouldThrowArgumentException()
    {
        // Arrange
        var entities = new List<Entity> { null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.CreateMultipleAsync(entities));
    }

    [Fact]
    public async Task UpdateMultipleAsync_WithValidEntities_ShouldUpdateSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var contactId = Guid.NewGuid();
        
        var account = new Entity("account", accountId) { ["name"] = "Updated Account" };
        var contact = new Entity("contact", contactId) { ["firstname"] = "Jane" };
        var entities = new List<Entity> { account, contact };

        var expectedResponse = new ExecuteTransactionResponse();
        expectedResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new UpdateResponse(),
            new UpdateResponse()
        };

        _mockOrganizationService
            .Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Returns(expectedResponse);

        // Act
        var result = await _transactionManager.UpdateMultipleAsync(entities);

        // Assert
        Assert.NotNull(result);
        _mockOrganizationService.Verify(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMultipleAsync_WithEmptyEntityId_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Entity("account") { ["name"] = "Test Account" };
        var entities = new List<Entity> { account };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.UpdateMultipleAsync(entities));
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithValidEntityReferences_ShouldDeleteSuccessfully()
    {
        // Arrange
        var accountRef = new EntityReference("account", Guid.NewGuid());
        var contactRef = new EntityReference("contact", Guid.NewGuid());
        var entityReferences = new List<EntityReference> { accountRef, contactRef };

        var expectedResponse = new ExecuteTransactionResponse();
        expectedResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new DeleteResponse(),
            new DeleteResponse()
        };

        _mockOrganizationService
            .Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Returns(expectedResponse);

        // Act
        var result = await _transactionManager.DeleteMultipleAsync(entityReferences);

        // Assert
        Assert.NotNull(result);
        _mockOrganizationService.Verify(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()), Times.Once);
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithNullEntityReferences_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _transactionManager.DeleteMultipleAsync(null!));
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithEmptyEntityReferences_ShouldThrowArgumentException()
    {
        // Arrange
        var entityReferences = new List<EntityReference>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.DeleteMultipleAsync(entityReferences));
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithNullEntityReferenceInList_ShouldThrowArgumentException()
    {
        // Arrange
        var entityReferences = new List<EntityReference> { null! };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.DeleteMultipleAsync(entityReferences));
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithEmptyLogicalName_ShouldThrowArgumentException()
    {
        // Arrange
        var entityRef = new EntityReference("", Guid.NewGuid());
        var entityReferences = new List<EntityReference> { entityRef };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.DeleteMultipleAsync(entityReferences));
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithEmptyEntityId_ShouldThrowArgumentException()
    {
        // Arrange
        var entityRef = new EntityReference("account", Guid.Empty);
        var entityReferences = new List<EntityReference> { entityRef };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _transactionManager.DeleteMultipleAsync(entityReferences));
    }

    [Fact]
    public async Task ExecuteTransactionAsync_WhenServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        var createRequest = new CreateRequest
        {
            Target = new Entity("account") { ["name"] = "Test Account" }
        };
        var requests = new List<OrganizationRequest> { createRequest };

        _mockOrganizationService
            .Setup(s => s.Execute(It.IsAny<ExecuteTransactionRequest>()))
            .Throws(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _transactionManager.ExecuteTransactionAsync(requests));
    }
}