using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace DataVerseCRUDOperations.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for the DataverseRepository class.
/// </summary>
public class DataverseRepositoryTests
{
    private readonly Mock<IOrganizationService> _mockOrganizationService;
    private readonly DataverseRepository<Account> _repository;

    public DataverseRepositoryTests()
    {
        _mockOrganizationService = new Mock<IOrganizationService>();
        _repository = new DataverseRepository<Account>(_mockOrganizationService.Object);
    }

    [Fact]
    public void Constructor_WithNullOrganizationService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DataverseRepository<Account>(null!));
    }

    [Fact]
    public async Task CreateAsync_WithValidEntity_ShouldReturnNewId()
    {
        // Arrange
        var account = new Account { Name = "Test Account" };
        var expectedId = Guid.NewGuid();

        _mockOrganizationService
            .Setup(s => s.Create(It.IsAny<Entity>()))
            .Returns(expectedId);

        // Act
        var result = await _repository.CreateAsync(account);

        // Assert
        Assert.Equal(expectedId, result);
        _mockOrganizationService.Verify(s => s.Create(It.Is<Entity>(e => 
            e.LogicalName == "account" && 
            e.GetAttributeValue<string>("name") == "Test Account")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.CreateAsync(null!));
    }

    [Fact]
    public async Task RetrieveAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new Entity("account", entityId);
        entity["name"] = "Test Account";
        entity["accountnumber"] = "ACC-001";

        _mockOrganizationService
            .Setup(s => s.Retrieve("account", entityId, It.IsAny<ColumnSet>()))
            .Returns(entity);

        // Act
        var result = await _repository.RetrieveAsync(entityId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entityId, result.Id);
        Assert.Equal("Test Account", result.Name);
        Assert.Equal("ACC-001", result.AccountNumber);
    }

    [Fact]
    public async Task RetrieveAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.RetrieveAsync(Guid.Empty));
    }

    [Fact]
    public async Task RetrieveAsync_WhenEntityNotFound_ShouldReturnNull()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        _mockOrganizationService
            .Setup(s => s.Retrieve("account", entityId, It.IsAny<ColumnSet>()))
            .Throws(new Exception("Entity does not exist"));

        // Act
        var result = await _repository.RetrieveAsync(entityId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldCallOrganizationService()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Updated Account"
        };

        // Act
        await _repository.UpdateAsync(account);

        // Assert
        _mockOrganizationService.Verify(s => s.Update(It.Is<Entity>(e => 
            e.LogicalName == "account" && 
            e.Id == account.Id &&
            e.GetAttributeValue<string>("name") == "Updated Account")), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Account" }; // Id is empty

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.UpdateAsync(account));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldCallOrganizationService()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        // Act
        await _repository.DeleteAsync(entityId);

        // Assert
        _mockOrganizationService.Verify(s => s.Delete("account", entityId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.DeleteAsync(Guid.Empty));
    }

    [Fact]
    public async Task RetrieveMultipleAsync_WithQueryExpression_ShouldReturnEntities()
    {
        // Arrange
        var query = new QueryExpression("account")
        {
            ColumnSet = new ColumnSet("name")
        };

        var entityCollection = new EntityCollection();
        var entity1 = new Entity("account", Guid.NewGuid());
        entity1["name"] = "Account 1";
        var entity2 = new Entity("account", Guid.NewGuid());
        entity2["name"] = "Account 2";
        entityCollection.Entities.Add(entity1);
        entityCollection.Entities.Add(entity2);

        _mockOrganizationService
            .Setup(s => s.RetrieveMultiple(query))
            .Returns(entityCollection);

        // Act
        var result = await _repository.RetrieveMultipleAsync(query);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.Name == "Account 1");
        Assert.Contains(result, a => a.Name == "Account 2");
    }

    [Fact]
    public async Task RetrieveMultipleAsync_WithWrongEntityName_ShouldThrowArgumentException()
    {
        // Arrange
        var query = new QueryExpression("contact"); // Wrong entity type

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.RetrieveMultipleAsync(query));
    }

    [Fact]
    public async Task RetrieveMultipleAsync_WithCondition_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entityCollection = new EntityCollection();
        var entity = new Entity("account", Guid.NewGuid());
        entity["name"] = "Test Account";
        entityCollection.Entities.Add(entity);

        _mockOrganizationService
            .Setup(s => s.RetrieveMultiple(It.Is<QueryExpression>(q => 
                q.EntityName == "account" &&
                q.Criteria.Conditions.Any(c => c.AttributeName == "name"))))
            .Returns(entityCollection);

        // Act
        var result = await _repository.RetrieveMultipleAsync("name", ConditionOperator.Equal, "Test Account");

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Account", result.First().Name);
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityExists_ShouldReturnTrue()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new Entity("account", entityId);

        _mockOrganizationService
            .Setup(s => s.Retrieve("account", entityId, It.IsAny<ColumnSet>()))
            .Returns(entity);

        // Act
        var result = await _repository.ExistsAsync(entityId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsAsync_WhenEntityDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        _mockOrganizationService
            .Setup(s => s.Retrieve("account", entityId, It.IsAny<ColumnSet>()))
            .Throws(new Exception("Entity does not exist"));

        // Act
        var result = await _repository.ExistsAsync(entityId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsAsync_WithEmptyId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.ExistsAsync(Guid.Empty);

        // Assert
        Assert.False(result);
    }
}