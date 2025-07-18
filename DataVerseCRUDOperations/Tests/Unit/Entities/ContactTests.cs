using DataVerseCRUDOperations.Entities;
using Microsoft.Xrm.Sdk;
using Xunit;

namespace DataVerseCRUDOperations.Tests.Unit.Entities;

/// <summary>
/// Unit tests for the Contact early bound entity.
/// </summary>
public class ContactTests
{
    [Fact]
    public void Contact_LogicalName_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var contact = new Contact();
        
        // Assert
        Assert.Equal("contact", contact.LogicalName);
    }

    [Fact]
    public void Contact_ToEntity_ShouldPopulateAllAttributes()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            EmailAddress1 = "john.doe@example.com",
            Telephone1 = "555-0123",
            MobilePhone = "555-0456",
            JobTitle = "Software Developer",
            ParentCustomerId = accountId
        };

        // Act
        var entity = contact.ToEntity();

        // Assert
        Assert.Equal("contact", entity.LogicalName);
        Assert.Equal(contact.Id, entity.Id);
        Assert.Equal("John", entity.GetAttributeValue<string>("firstname"));
        Assert.Equal("Doe", entity.GetAttributeValue<string>("lastname"));
        Assert.Equal("john.doe@example.com", entity.GetAttributeValue<string>("emailaddress1"));
        Assert.Equal("555-0123", entity.GetAttributeValue<string>("telephone1"));
        Assert.Equal("555-0456", entity.GetAttributeValue<string>("mobilephone"));
        Assert.Equal("Software Developer", entity.GetAttributeValue<string>("jobtitle"));
        
        var parentCustomer = entity.GetAttributeValue<EntityReference>("parentcustomerid");
        Assert.NotNull(parentCustomer);
        Assert.Equal("account", parentCustomer.LogicalName);
        Assert.Equal(accountId, parentCustomer.Id);
    }

    [Fact]
    public void Contact_ToEntity_ShouldSkipNullValues()
    {
        // Arrange
        var contact = new Contact
        {
            LastName = "Doe"
            // Other properties are null
        };

        // Act
        var entity = contact.ToEntity();

        // Assert
        Assert.Equal("Doe", entity.GetAttributeValue<string>("lastname"));
        Assert.False(entity.Contains("firstname"));
        Assert.False(entity.Contains("emailaddress1"));
        Assert.False(entity.Contains("telephone1"));
        Assert.False(entity.Contains("parentcustomerid"));
    }

    [Fact]
    public void Contact_FromEntity_ShouldPopulateAllProperties()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var entity = new Entity("contact", entityId);
        entity["firstname"] = "John";
        entity["lastname"] = "Doe";
        entity["fullname"] = "John Doe";
        entity["emailaddress1"] = "john.doe@example.com";
        entity["telephone1"] = "555-0123";
        entity["mobilephone"] = "555-0456";
        entity["jobtitle"] = "Software Developer";
        entity["parentcustomerid"] = new EntityReference("account", accountId);
        entity["createdon"] = DateTime.Now;
        entity["modifiedon"] = DateTime.Now.AddDays(-1);

        var contact = new Contact();

        // Act
        contact.FromEntity(entity);

        // Assert
        Assert.Equal(entityId, contact.Id);
        Assert.Equal("John", contact.FirstName);
        Assert.Equal("Doe", contact.LastName);
        Assert.Equal("John Doe", contact.FullName);
        Assert.Equal("john.doe@example.com", contact.EmailAddress1);
        Assert.Equal("555-0123", contact.Telephone1);
        Assert.Equal("555-0456", contact.MobilePhone);
        Assert.Equal("Software Developer", contact.JobTitle);
        Assert.Equal(accountId, contact.ParentCustomerId);
        Assert.NotNull(contact.CreatedOn);
        Assert.NotNull(contact.ModifiedOn);
    }

    [Fact]
    public void Contact_FromEntity_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        var contact = new Contact();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => contact.FromEntity(null!));
    }

    [Fact]
    public void Contact_FromEntity_WithWrongEntityType_ShouldThrowArgumentException()
    {
        // Arrange
        var contact = new Contact();
        var accountEntity = new Entity("account");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => contact.FromEntity(accountEntity));
        Assert.Contains("Entity logical name 'account' does not match expected 'contact'", exception.Message);
    }

    [Fact]
    public void Contact_FromEntity_WithMissingAttributes_ShouldHandleGracefully()
    {
        // Arrange
        var contact = new Contact();
        var entity = new Entity("contact", Guid.NewGuid());
        entity["lastname"] = "Doe";
        // Other attributes are missing

        // Act
        contact.FromEntity(entity);

        // Assert
        Assert.Equal("Doe", contact.LastName);
        Assert.Null(contact.FirstName);
        Assert.Null(contact.EmailAddress1);
        Assert.Null(contact.ParentCustomerId);
    }
}