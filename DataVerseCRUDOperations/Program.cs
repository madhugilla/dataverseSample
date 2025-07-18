namespace DataVerseCRUDOperations;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using DataVerseCRUDOperations.Entities;
using DataVerseCRUDOperations.Repositories;
using DataVerseCRUDOperations.Services;

public class Program
{
    // TODO Enter your Dataverse environment's URL and logon info.
    static string url = "https://yourorg.crm.dynamics.com";
    static string userName = "you@yourorg.onmicrosoft.com";
    static string password = "yourPassword";

    // This service connection string uses the info provided above.
    // The AppId and RedirectUri are provided for sample code testing.
    static string connectionString = $@"
   AuthType = OAuth;
   Url = {url};
   UserName = {userName};
   Password = {password};
   AppId = 51f81489-12ee-4a9e-aaae-a2591f45987d;
   RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;
   LoginPrompt=Auto;
   RequireNewInstance = True";

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Dataverse Early Bound CRUD Operations Demo ===\n");

            // 1. **Authenticate and connect to Dataverse**  
            string url, clientId, clientSecret;
            GetConfig(out url, out clientId, out clientSecret);
            string connectionString = $@"
                AuthType=ClientSecret;
                Url={url};
                ClientId={clientId};
                ClientSecret={clientSecret};
            ";

            // Initialize the ServiceClient
            ServiceClient serviceClient = new ServiceClient(connectionString);
            IOrganizationService service = serviceClient;

            var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());
            Console.WriteLine($"Connected to Dataverse. User ID: {response.UserId}\n");

            // 2. **Demonstrate Early Bound Implementation**
            await DemonstrateEarlyBoundOperations(service);

            Console.WriteLine("\nDemo completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }

    /// <summary>
    /// Demonstrates the early bound implementation with type-safe operations.
    /// </summary>
    private static async Task DemonstrateEarlyBoundOperations(IOrganizationService service)
    {
        // Initialize repositories and services using dependency injection pattern
        var accountRepository = new DataverseRepository<Account>(service);
        var contactRepository = new DataverseRepository<Contact>(service);
        
        var accountService = new AccountService(accountRepository);
        var contactService = new ContactService(contactRepository);

        Console.WriteLine("--- Early Bound Account Operations ---");
        
        // Create a new Account using early bound entity
        var newAccount = new Account
        {
            Name = "Early Bound Sample Account",
            AccountNumber = "EB-001",
            Telephone1 = "555-0123",
            WebSiteUrl = "https://example.com",
            Description = "Account created using early bound implementation",
            NumberOfEmployees = 100,
            Revenue = 1000000m
        };

        Console.WriteLine("Creating new account...");
        var createdAccount = await accountService.CreateAsync(newAccount);
        Console.WriteLine($"Created Account: {createdAccount.Name} (ID: {createdAccount.Id})");

        // Retrieve the account
        Console.WriteLine("\nRetrieving account by ID...");
        var retrievedAccount = await accountService.GetByIdAsync(createdAccount.Id, "name", "accountnumber", "revenue");
        if (retrievedAccount != null)
        {
            Console.WriteLine($"Retrieved Account: {retrievedAccount.Name} - {retrievedAccount.AccountNumber}");
            Console.WriteLine($"Revenue: ${retrievedAccount.Revenue:N2}");
        }

        // Update the account
        Console.WriteLine("\nUpdating account...");
        retrievedAccount!.Name = "Updated Early Bound Account";
        retrievedAccount.Revenue = 1500000m;
        var updatedAccount = await accountService.UpdateAsync(retrievedAccount);
        Console.WriteLine($"Updated Account: {updatedAccount.Name} - Revenue: ${updatedAccount.Revenue:N2}");

        // Search accounts by name
        Console.WriteLine("\nSearching accounts by name...");
        var searchResults = await accountService.SearchByNameAsync("Early Bound", exactMatch: false);
        Console.WriteLine($"Found {searchResults.Count()} account(s) with 'Early Bound' in name:");
        foreach (var account in searchResults)
        {
            Console.WriteLine($"  - {account.Name} ({account.AccountNumber})");
        }

        // Get high revenue accounts
        Console.WriteLine("\nGetting high revenue accounts (> $500,000)...");
        var highRevenueAccounts = await accountService.GetHighRevenueAccountsAsync(500000m);
        Console.WriteLine($"Found {highRevenueAccounts.Count()} high revenue account(s):");
        foreach (var account in highRevenueAccounts)
        {
            Console.WriteLine($"  - {account.Name}: ${account.Revenue:N2}");
        }

        Console.WriteLine("\n--- Early Bound Contact Operations ---");
        
        // Create a new Contact using early bound entity
        var newContact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            EmailAddress1 = "john.doe@example.com",
            Telephone1 = "555-0456",
            JobTitle = "Software Developer",
            ParentCustomerId = createdAccount.Id
        };

        Console.WriteLine("Creating new contact...");
        var createdContact = await contactService.CreateAsync(newContact);
        Console.WriteLine($"Created Contact: {createdContact.FirstName} {createdContact.LastName} (ID: {createdContact.Id})");

        // Get contact by email
        Console.WriteLine("\nRetrieving contact by email...");
        var contactByEmail = await contactService.GetByEmailAsync("john.doe@example.com");
        if (contactByEmail != null)
        {
            Console.WriteLine($"Found Contact: {contactByEmail.FirstName} {contactByEmail.LastName} - {contactByEmail.JobTitle}");
        }

        // Get contacts by account
        Console.WriteLine($"\nGetting contacts for account: {createdAccount.Name}...");
        var accountContacts = await contactService.GetContactsByAccountAsync(createdAccount.Id);
        Console.WriteLine($"Found {accountContacts.Count()} contact(s) for this account:");
        foreach (var contact in accountContacts)
        {
            Console.WriteLine($"  - {contact.FirstName} {contact.LastName} ({contact.EmailAddress1})");
        }

        // Search contacts by name
        Console.WriteLine("\nSearching contacts by last name 'Doe'...");
        var contactSearchResults = await contactService.SearchByNameAsync("Doe");
        Console.WriteLine($"Found {contactSearchResults.Count()} contact(s) with 'Doe' in name:");
        foreach (var contact in contactSearchResults)
        {
            Console.WriteLine($"  - {contact.FirstName} {contact.LastName} ({contact.EmailAddress1})");
        }

        // Clean up - Delete the created records
        Console.WriteLine("\n--- Cleanup Operations ---");
        
        Console.WriteLine("Deleting contact...");
        var contactDeleted = await contactService.DeleteAsync(createdContact.Id);
        Console.WriteLine($"Contact deleted: {contactDeleted}");

        Console.WriteLine("Deleting account...");
        var accountDeleted = await accountService.DeleteAsync(createdAccount.Id);
        Console.WriteLine($"Account deleted: {accountDeleted}");

        Console.WriteLine("\nEarly bound operations completed successfully!");
    }

    private static void GetConfig(out string url, out string clientId, out string clientSecret)
    {
        // Read configuration from user secrets
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        url = config["Dataverse:Url"] ?? throw new InvalidOperationException("Dataverse:Url is not configured in user secrets");
        clientId = config["Dataverse:ClientId"] ?? throw new InvalidOperationException("Dataverse:ClientId is not configured in user secrets");
        clientSecret = config["Dataverse:ClientSecret"] ?? throw new InvalidOperationException("Dataverse:ClientSecret is not configured in user secrets");
    }
}