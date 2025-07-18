# Dataverse Early Bound Implementation

This project demonstrates a clean, maintainable early bound implementation for Microsoft Dataverse operations using C# and the PowerPlatform.Dataverse.Client SDK.

## Architecture Overview

The implementation follows clean architecture principles with the following layers:

```
DataVerseCRUDOperations/
├── Entities/              # Early bound entity classes
│   ├── IDataverseEntity.cs     # Base interface for all entities
│   ├── DataverseEntityBase.cs  # Abstract base class
│   ├── Account.cs              # Account entity implementation
│   └── Contact.cs              # Contact entity implementation
├── Repositories/          # Generic repository pattern
│   ├── IRepository.cs          # Generic repository interface
│   └── DataverseRepository.cs  # Generic repository implementation
├── Services/              # Business logic layer
│   ├── IDataverseService.cs    # Generic service interface
│   ├── DataverseService.cs     # Generic service implementation
│   ├── AccountService.cs       # Account-specific business logic
│   └── ContactService.cs       # Contact-specific business logic
├── Tests/                 # Comprehensive test suite
│   ├── Unit/                   # Unit tests with mocking
│   └── Integration/            # Integration tests
└── Program.cs             # Updated demonstration program
```

## Key Features

### 1. Early Bound Entities
- **Type Safety**: Strongly-typed entities replace string-based late bound operations
- **IntelliSense Support**: Full IDE support with auto-completion and compile-time checking
- **Validation**: Built-in property validation and type conversion
- **Extensible**: Easy to add new entity types

### 2. Generic Repository Pattern
- **CRUD Operations**: Type-safe Create, Read, Update, Delete operations
- **Query Support**: Advanced querying with QueryExpression and simple conditions
- **Async/Await**: Modern asynchronous programming patterns
- **Error Handling**: Comprehensive error handling and validation

### 3. Service Layer
- **Business Logic**: Centralized business rules and validation
- **Entity-Specific Services**: Specialized services for each entity type
- **Search Operations**: Advanced search and filtering capabilities
- **Dependency Injection Ready**: Clean separation of concerns

### 4. Comprehensive Testing
- **Unit Tests**: 51 unit tests covering all components with mocking
- **Integration Tests**: End-to-end testing against real Dataverse
- **Test Coverage**: High test coverage ensuring reliability
- **Automated Testing**: CI/CD ready test suite

## Benefits Over Late Bound Implementation

| Aspect | Late Bound | Early Bound |
|--------|------------|-------------|
| Type Safety | ❌ String-based | ✅ Strongly typed |
| IntelliSense | ❌ Limited | ✅ Full support |
| Compile-time Checking | ❌ No | ✅ Yes |
| Refactoring Support | ❌ Manual | ✅ Automated |
| Performance | ⚠️ Runtime validation | ✅ Compile-time validation |
| Maintainability | ❌ Error-prone | ✅ Easy to maintain |

## Quick Start

### 1. Prerequisites
- .NET 8.0 or later
- Access to a Microsoft Dataverse environment
- Azure AD app registration with appropriate permissions

### 2. Configuration
```bash
cd DataVerseCRUDOperations
dotnet user-secrets set "Dataverse:Url" "https://yourorg.crm.dynamics.com"
dotnet user-secrets set "Dataverse:ClientId" "your-client-id"
dotnet user-secrets set "Dataverse:ClientSecret" "your-client-secret"
```

### 3. Build and Run
```bash
dotnet build
dotnet run
```

### 4. Run Tests
```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests (requires Dataverse configuration)
dotnet test --filter "FullyQualifiedName~Integration"
```

## Usage Examples

### Creating and Managing Accounts

```csharp
// Initialize services
var accountRepository = new DataverseRepository<Account>(organizationService);
var accountService = new AccountService(accountRepository);

// Create a new account
var account = new Account
{
    Name = "Contoso Corporation",
    AccountNumber = "CONT-001",
    Revenue = 1000000m,
    NumberOfEmployees = 100
};

var createdAccount = await accountService.CreateAsync(account);

// Search for accounts
var accounts = await accountService.SearchByNameAsync("Contoso", exactMatch: false);

// Get high revenue accounts
var highRevenueAccounts = await accountService.GetHighRevenueAccountsAsync(500000m);

// Update account
account.Revenue = 1500000m;
await accountService.UpdateAsync(account);

// Delete account
await accountService.DeleteAsync(account.Id);
```

### Working with Contacts

```csharp
// Initialize services
var contactRepository = new DataverseRepository<Contact>(organizationService);
var contactService = new ContactService(contactRepository);

// Create a contact
var contact = new Contact
{
    FirstName = "John",
    LastName = "Doe",
    EmailAddress1 = "john.doe@contoso.com",
    ParentCustomerId = accountId
};

var createdContact = await contactService.CreateAsync(contact);

// Find contact by email
var contactByEmail = await contactService.GetByEmailAsync("john.doe@contoso.com");

// Get all contacts for an account
var accountContacts = await contactService.GetContactsByAccountAsync(accountId);
```

### Generic Repository Usage

```csharp
// Works with any entity type
var repository = new DataverseRepository<T>(organizationService);

// CRUD operations
var id = await repository.CreateAsync(entity);
var retrieved = await repository.RetrieveAsync(id);
await repository.UpdateAsync(entity);
await repository.DeleteAsync(id);

// Query operations
var results = await repository.RetrieveMultipleAsync(query);
var filtered = await repository.RetrieveMultipleAsync("attributeName", ConditionOperator.Equal, value);
```

## Extending the Implementation

### Adding New Entity Types

1. Create entity class inheriting from `DataverseEntityBase`
2. Implement required abstract methods
3. Add entity-specific properties
4. Create specialized service if needed

```csharp
public class CustomEntity : DataverseEntityBase
{
    public override string LogicalName => "new_customentity";
    
    public string? CustomField { get; set; }
    
    protected override void PopulateEntity(Entity entity)
    {
        if (!string.IsNullOrEmpty(CustomField))
            entity["new_customfield"] = CustomField;
    }
    
    protected override void PopulateFromEntity(Entity entity)
    {
        CustomField = entity.GetAttributeValue<string>("new_customfield");
    }
}
```

### Custom Validation

Override the `ValidateEntity` method in service classes:

```csharp
protected override void ValidateEntity(CustomEntity entity)
{
    base.ValidateEntity(entity);
    
    if (string.IsNullOrEmpty(entity.CustomField))
        throw new ArgumentException("Custom field is required");
}
```

## Best Practices Implemented

1. **Separation of Concerns**: Clear separation between data access, business logic, and presentation
2. **Dependency Injection**: Interfaces allow for easy testing and flexibility
3. **Async/Await**: Modern asynchronous programming patterns
4. **Error Handling**: Comprehensive error handling with meaningful messages
5. **Validation**: Input validation at multiple layers
6. **Testing**: Comprehensive unit and integration test coverage
7. **Documentation**: Clear documentation and code comments
8. **SOLID Principles**: Adherence to SOLID design principles

## Performance Considerations

- **Async Operations**: All operations are asynchronous for better scalability
- **Column Selection**: Option to specify columns for better query performance
- **Connection Reuse**: Single connection instance for multiple operations
- **Batch Operations**: Support for bulk operations through QueryExpression

## Testing Strategy

The implementation includes comprehensive testing:

- **Unit Tests**: Test business logic in isolation using mocks
- **Integration Tests**: Test end-to-end functionality against real Dataverse
- **Validation Tests**: Ensure proper error handling and validation
- **Edge Case Testing**: Handle null values, empty collections, etc.

## Dependencies

- **Microsoft.PowerPlatform.Dataverse.Client**: Main SDK for Dataverse operations
- **Microsoft.Extensions.Configuration.UserSecrets**: Secure configuration management
- **xUnit**: Testing framework
- **Moq**: Mocking framework for unit tests

## License

This project is provided as a sample for educational and demonstration purposes.