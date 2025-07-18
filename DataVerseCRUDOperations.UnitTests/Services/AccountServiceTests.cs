using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using DataVerseCRUDOperations.Services;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using Xunit;

namespace DataVerseCRUDOperations.UnitTests.Services;

/// <summary>
/// Unit tests for the AccountService class.
/// </summary>
public class AccountServiceTests
{
    private readonly Mock<IRepository<Account>> _mockRepository;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _mockRepository = new Mock<IRepository<Account>>();
        _accountService = new AccountService(_mockRepository.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AccountService(null!));
    }

    [Fact]
    public async Task CreateAsync_WithValidAccount_ShouldReturnAccountWithId()
    {
        // Arrange
        var account = new Account { Name = "Test Account" };
        var expectedId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.CreateAsync(account))
            .ReturnsAsync(expectedId);

        // Act
        var result = await _accountService.CreateAsync(account);

        // Assert
        Assert.Equal(expectedId, result.Id);
        Assert.Equal("Test Account", result.Name);
        _mockRepository.Verify(r => r.CreateAsync(account), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithNullAccount_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _accountService.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidAccount_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account(); // Name is null/empty

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAsync(account));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string name)
    {
        // Arrange
        var account = new Account { Name = name };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAsync(account));
    }

    [Fact]
    public async Task CreateAsync_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = new string('A', 161) }; // Over 160 characters

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAsync(account));
    }

    [Fact]
    public async Task CreateAsync_WithNegativeRevenue_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Account", Revenue = -100 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAsync(account));
    }

    [Fact]
    public async Task CreateAsync_WithNegativeNumberOfEmployees_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account { Name = "Test Account", NumberOfEmployees = -5 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.CreateAsync(account));
    }

    [Fact]
    public async Task UpdateAsync_WithValidAccount_ShouldReturnUpdatedAccount()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Updated Account"
        };

        _mockRepository
            .Setup(r => r.ExistsAsync(account.Id))
            .ReturnsAsync(true);

        // Act
        var result = await _accountService.UpdateAsync(account);

        // Assert
        Assert.Equal(account.Id, result.Id);
        Assert.Equal("Updated Account", result.Name);
        _mockRepository.Verify(r => r.UpdateAsync(account), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentAccount_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Test Account"
        };

        _mockRepository
            .Setup(r => r.ExistsAsync(account.Id))
            .ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.UpdateAsync(account));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingAccount_ShouldReturnTrue()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.ExistsAsync(accountId))
            .ReturnsAsync(true);

        // Act
        var result = await _accountService.DeleteAsync(accountId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(accountId), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentAccount_ShouldReturnFalse()
    {
        // Arrange
        var accountId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.ExistsAsync(accountId))
            .ReturnsAsync(false);

        // Act
        var result = await _accountService.DeleteAsync(accountId);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SearchByNameAsync_WithExactMatch_ShouldReturnMatchingAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Name = "Test Account", AccountNumber = "ACC-001" }
        };

        _mockRepository
            .Setup(r => r.RetrieveMultipleAsync("name", ConditionOperator.Equal, "Test Account", It.IsAny<ColumnSet>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _accountService.SearchByNameAsync("Test Account", exactMatch: true);

        // Assert
        Assert.Single(result);
        Assert.Equal("Test Account", result.First().Name);
    }

    [Fact]
    public async Task SearchByNameAsync_WithPartialMatch_ShouldReturnMatchingAccounts()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Name = "Test Account 1" },
            new Account { Name = "Test Account 2" }
        };

        _mockRepository
            .Setup(r => r.RetrieveMultipleAsync("name", ConditionOperator.BeginsWith, "Test", It.IsAny<ColumnSet>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _accountService.SearchByNameAsync("Test", exactMatch: false);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchByNameAsync_WithNullOrEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.SearchByNameAsync(null!));
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.SearchByNameAsync(""));
    }

    [Fact]
    public async Task GetHighRevenueAccountsAsync_ShouldReturnAccountsAboveThreshold()
    {
        // Arrange
        var highRevenueAccounts = new List<Account>
        {
            new Account { Name = "High Revenue Account 1", Revenue = 600000 },
            new Account { Name = "High Revenue Account 2", Revenue = 800000 }
        };

        _mockRepository
            .Setup(r => r.RetrieveMultipleAsync(It.Is<QueryExpression>(q => 
                q.EntityName == "account" &&
                q.Criteria.Conditions.Any(c => c.AttributeName == "revenue" && c.Operator == ConditionOperator.GreaterThan))))
            .ReturnsAsync(highRevenueAccounts);

        // Act
        var result = await _accountService.GetHighRevenueAccountsAsync(500000);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, account => Assert.True(account.Revenue > 500000));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnAccount()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var account = new Account { Id = accountId, Name = "Test Account" };

        _mockRepository
            .Setup(r => r.RetrieveAsync(accountId, It.IsAny<ColumnSet>()))
            .ReturnsAsync(account);

        // Act
        var result = await _accountService.GetByIdAsync(accountId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(accountId, result.Id);
        Assert.Equal("Test Account", result.Name);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnRepositoryResult()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.ExistsAsync(accountId))
            .ReturnsAsync(true);

        // Act
        var result = await _accountService.ExistsAsync(accountId);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.ExistsAsync(accountId), Times.Once);
    }
}