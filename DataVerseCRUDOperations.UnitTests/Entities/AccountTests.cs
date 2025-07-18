using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace DataVerseCRUDOperations.UnitTests.Entities;

/// <summary>
/// Unit tests for the Account early bound entity.
/// </summary>
public class AccountTests
{
    [Fact]
    public void Account_LogicalName_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var account = new Account();
        
        // Assert
        Assert.Equal("account", account.LogicalName);
    }

    [Fact]
    public void Account_ToEntity_ShouldPopulateAllAttributes()
    {
        // Arrange
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Test Account",
            AccountNumber = "ACC-001",
            Telephone1 = "555-0123",
            WebSiteUrl = "https://test.com",
            Description = "Test Description",
            NumberOfEmployees = 100,
            Revenue = 1000000m
        };

        // Act
        var entity = account.ToEntity();

        // Assert
        Assert.Equal("account", entity.LogicalName);
        Assert.Equal(account.Id, entity.Id);
        Assert.Equal("Test Account", entity.GetAttributeValue<string>("name"));
        Assert.Equal("ACC-001", entity.GetAttributeValue<string>("accountnumber"));
        Assert.Equal("555-0123", entity.GetAttributeValue<string>("telephone1"));
        Assert.Equal("https://test.com", entity.GetAttributeValue<string>("websiteurl"));
        Assert.Equal("Test Description", entity.GetAttributeValue<string>("description"));
        Assert.Equal(100, entity.GetAttributeValue<int>("numberofemployees"));
        Assert.Equal(1000000m, entity.GetAttributeValue<Money>("revenue").Value);
    }

    [Fact]
    public void Account_ToEntity_ShouldSkipNullValues()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account"
            // Other properties are null
        };

        // Act
        var entity = account.ToEntity();

        // Assert
        Assert.Equal("Test Account", entity.GetAttributeValue<string>("name"));
        Assert.False(entity.Contains("accountnumber"));
        Assert.False(entity.Contains("telephone1"));
        Assert.False(entity.Contains("numberofemployees"));
        Assert.False(entity.Contains("revenue"));
    }

    [Fact]
    public void Account_FromEntity_ShouldPopulateAllProperties()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new Entity("account", entityId);
        entity["name"] = "Test Account";
        entity["accountnumber"] = "ACC-001";
        entity["telephone1"] = "555-0123";
        entity["websiteurl"] = "https://test.com";
        entity["description"] = "Test Description";
        entity["numberofemployees"] = 100;
        entity["revenue"] = new Money(1000000m);
        entity["createdon"] = DateTime.Now;
        entity["modifiedon"] = DateTime.Now.AddDays(-1);

        var account = new Account();

        // Act
        account.FromEntity(entity);

        // Assert
        Assert.Equal(entityId, account.Id);
        Assert.Equal("Test Account", account.Name);
        Assert.Equal("ACC-001", account.AccountNumber);
        Assert.Equal("555-0123", account.Telephone1);
        Assert.Equal("https://test.com", account.WebSiteUrl);
        Assert.Equal("Test Description", account.Description);
        Assert.Equal(100, account.NumberOfEmployees);
        Assert.Equal(1000000m, account.Revenue);
        Assert.NotNull(account.CreatedOn);
        Assert.NotNull(account.ModifiedOn);
    }

    [Fact]
    public void Account_FromEntity_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var account = new Account();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => account.FromEntity(null!));
    }

    [Fact]
    public void Account_FromEntity_WithWrongEntityType_ShouldThrowArgumentException()
    {
        // Arrange
        var account = new Account();
        var contactEntity = new Entity("contact");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => account.FromEntity(contactEntity));
        Assert.Contains("Entity logical name 'contact' does not match expected 'account'", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Account_FromEntity_WithMissingAttributes_ShouldHandleGracefully(string? value)
    {
        // Arrange
        var account = new Account();
        var entity = new Entity("account", Guid.NewGuid());
        if (value != null)
        {
            entity["name"] = value;
        }

        // Act
        account.FromEntity(entity);

        // Assert
        Assert.Equal(value, account.Name);
        Assert.Null(account.AccountNumber);
        Assert.Null(account.Revenue);
        Assert.Null(account.NumberOfEmployees);
    }
}