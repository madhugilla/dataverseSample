using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using DataVerseCRUDOperations.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Moq;
using Xunit;

namespace DataVerseCRUDOperations.UnitTests.Services;

/// <summary>
/// Unit tests for the TransactionalService class.
/// </summary>
public class TransactionalServiceTests
{
    private readonly Mock<IRepository<Account>> _mockAccountRepository;
    private readonly Mock<IRepository<Contact>> _mockContactRepository;
    private readonly TransactionalService _transactionalService;

    public TransactionalServiceTests()
    {
        _mockAccountRepository = new Mock<IRepository<Account>>();
        _mockContactRepository = new Mock<IRepository<Contact>>();
        _transactionalService = new TransactionalService(_mockAccountRepository.Object, _mockContactRepository.Object);
    }

    [Fact]
    public async Task CreateMultipleAsync_WithValidEntities_ShouldCreateSuccessfully()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Revenue = 1000000m
        };

        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress1 = "john.doe@example.com"
        };

        var accountId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var mockResponse = new ExecuteTransactionResponse();
        mockResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new CreateResponse { Results = { ["id"] = accountId } },
            new CreateResponse { Results = { ["id"] = contactId } }
        };

        _mockAccountRepository
            .Setup(r => r.ExecuteTransactionAsync(It.IsAny<IEnumerable<OrganizationRequest>>()))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _transactionalService.CreateMultipleAsync(account, contact);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(accountId, result[account]);
        Assert.Equal(contactId, result[contact]);
        Assert.Equal(accountId, account.Id);
        Assert.Equal(contactId, contact.Id);

        _mockAccountRepository.Verify(
            r => r.ExecuteTransactionAsync(It.Is<IEnumerable<OrganizationRequest>>(
                requests => requests.Count() == 2 && 
                          requests.All(req => req is CreateRequest))),
            Times.Once);
    }

    [Fact]
    public async Task CreateMultipleAsync_WithNullEntities_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateMultipleAsync(null!));
        
        Assert.Contains("At least one entity is required", ex.Message);
    }

    [Fact]
    public async Task CreateMultipleAsync_WithEmptyEntities_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateMultipleAsync());
        
        Assert.Contains("At least one entity is required", ex.Message);
    }

    [Fact]
    public async Task CreateMultipleAsync_WithNullEntityInArray_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Account" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateMultipleAsync(account, null!));
        
        Assert.Contains("Entity cannot be null", ex.Message);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Company",
            Revenue = 5000000m
        };

        var contact1 = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress1 = "john.doe@testcompany.com"
        };

        var contact2 = new Contact
        {
            FirstName = "Jane",
            LastName = "Smith",
            EmailAddress1 = "jane.smith@testcompany.com"
        };

        var accountId = Guid.NewGuid();
        var contact1Id = Guid.NewGuid();
        var contact2Id = Guid.NewGuid();

        // Mock the first transaction (create entities)
        var mockCreateResponse = new ExecuteTransactionResponse();
        mockCreateResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new CreateResponse { Results = { ["id"] = accountId } },
            new CreateResponse { Results = { ["id"] = contact1Id } },
            new CreateResponse { Results = { ["id"] = contact2Id } }
        };

        // Mock the second transaction (link contacts to account)
        var mockUpdateResponse = new ExecuteTransactionResponse();
        mockUpdateResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new UpdateResponse(),
            new UpdateResponse()
        };

        _mockAccountRepository
            .Setup(r => r.ExecuteTransactionAsync(It.Is<IEnumerable<OrganizationRequest>>(
                requests => requests.Count() == 3 && requests.All(req => req is CreateRequest))))
            .ReturnsAsync(mockCreateResponse);

        _mockContactRepository
            .Setup(r => r.ExecuteTransactionAsync(It.Is<IEnumerable<OrganizationRequest>>(
                requests => requests.Count() == 2 && requests.All(req => req is UpdateRequest))))
            .ReturnsAsync(mockUpdateResponse);

        // Act
        var (resultAccountId, resultContactIds) = await _transactionalService.CreateAccountWithContactsAsync(
            account, contact1, contact2);

        // Assert
        Assert.Equal(accountId, resultAccountId);
        Assert.Equal(2, resultContactIds.Count);
        Assert.Contains(contact1Id, resultContactIds);
        Assert.Contains(contact2Id, resultContactIds);

        Assert.Equal(accountId, account.Id);
        Assert.Equal(contact1Id, contact1.Id);
        Assert.Equal(contact2Id, contact2.Id);
        Assert.Equal(accountId, contact1.ParentCustomerId);
        Assert.Equal(accountId, contact2.ParentCustomerId);

        _mockAccountRepository.Verify(
            r => r.ExecuteTransactionAsync(It.IsAny<IEnumerable<OrganizationRequest>>()),
            Times.Once);

        _mockContactRepository.Verify(
            r => r.ExecuteTransactionAsync(It.IsAny<IEnumerable<OrganizationRequest>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithNullAccount_ShouldThrowArgumentNullException()
    {
        // Arrange
        var contact = new Contact { FirstName = "John", LastName = "Doe" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _transactionalService.CreateAccountWithContactsAsync(null!, contact));
        
        Assert.Equal("account", ex.ParamName);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithNullContacts_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Company" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateAccountWithContactsAsync(account, null!));
        
        Assert.Contains("At least one contact is required", ex.Message);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithEmptyContacts_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Company" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateAccountWithContactsAsync(account));
        
        Assert.Contains("At least one contact is required", ex.Message);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithEmptyAccountName_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "" };
        var contact = new Contact { FirstName = "John", LastName = "Doe" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateAccountWithContactsAsync(account, contact));
        
        Assert.Contains("Account name is required", ex.Message);
    }

    [Fact]
    public async Task CreateAccountWithContactsAsync_WithContactMissingName_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Company" };
        var contact = new Contact { EmailAddress1 = "test@example.com" }; // No first or last name

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.CreateAccountWithContactsAsync(account, contact));
        
        Assert.Contains("Contact must have at least first name or last name", ex.Message);
    }

    [Fact]
    public async Task UpdateMultipleAsync_WithValidEntities_ShouldUpdateSuccessfully()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Updated Account",
            Revenue = 2000000m
        };

        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = "Updated John",
            LastName = "Updated Doe"
        };

        var mockResponse = new ExecuteTransactionResponse();
        mockResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new UpdateResponse(),
            new UpdateResponse()
        };

        _mockAccountRepository
            .Setup(r => r.ExecuteTransactionAsync(It.IsAny<IEnumerable<OrganizationRequest>>()))
            .ReturnsAsync(mockResponse);

        // Act
        await _transactionalService.UpdateMultipleAsync(account, contact);

        // Assert
        _mockAccountRepository.Verify(
            r => r.ExecuteTransactionAsync(It.Is<IEnumerable<OrganizationRequest>>(
                requests => requests.Count() == 2 && 
                          requests.All(req => req is UpdateRequest))),
            Times.Once);
    }

    [Fact]
    public async Task UpdateMultipleAsync_WithEntityMissingId_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Account" }; // No ID set

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.UpdateMultipleAsync(account));
        
        Assert.Contains("Entity ID cannot be empty for update operation", ex.Message);
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithValidReferences_ShouldDeleteSuccessfully()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var contactId = Guid.NewGuid();

        var mockResponse = new ExecuteTransactionResponse();
        mockResponse.Results["Responses"] = new OrganizationResponseCollection
        {
            new DeleteResponse(),
            new DeleteResponse()
        };

        _mockAccountRepository
            .Setup(r => r.ExecuteTransactionAsync(It.IsAny<IEnumerable<OrganizationRequest>>()))
            .ReturnsAsync(mockResponse);

        // Act
        await _transactionalService.DeleteMultipleAsync(
            ("account", accountId),
            ("contact", contactId));

        // Assert
        _mockAccountRepository.Verify(
            r => r.ExecuteTransactionAsync(It.Is<IEnumerable<OrganizationRequest>>(
                requests => requests.Count() == 2 && 
                          requests.All(req => req is DeleteRequest))),
            Times.Once);
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.DeleteMultipleAsync(("account", Guid.Empty)));
        
        Assert.Contains("Entity ID cannot be empty", ex.Message);
    }

    [Fact]
    public async Task DeleteMultipleAsync_WithEmptyLogicalName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _transactionalService.DeleteMultipleAsync(("", Guid.NewGuid())));
        
        Assert.Contains("Logical name cannot be null or empty", ex.Message);
    }

    [Fact]
    public void Constructor_WithNullAccountRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => new TransactionalService(null!, _mockContactRepository.Object));
        
        Assert.Equal("accountRepository", ex.ParamName);
    }

    [Fact]
    public void Constructor_WithNullContactRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(
            () => new TransactionalService(_mockAccountRepository.Object, null!));
        
        Assert.Equal("contactRepository", ex.ParamName);
    }
}