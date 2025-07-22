namespace DataVerseCRUDOperations;

using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

class Program
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

    static void Main()
    {
        // Run standalone tests first (no Dataverse connection required)
        Console.WriteLine("Running lookup operations tests...");
        LookupOperationsTest.RunAllTests();
        
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("Starting Dataverse connection demo...");
        Console.WriteLine(new string('=', 60));
        
        // //ServiceClient implements IOrganizationService interface
        // IOrganizationService service = new ServiceClient(connectionString);

        // var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

        // Console.WriteLine($"User ID is {response.UserId}.");

        // 1. **Authenticate and connect to Dataverse**  
        // Provide Dataverse environment URL and credentials for ServiceClient.
        string url, clientId, clientSecret;
        GetConfig(out url, out clientId, out clientSecret);// Client secret for the app
        string connectionString = $@"
            AuthType=ClientSecret;
            Url={url};
            ClientId={clientId};
            ClientSecret={clientSecret};
        ";
        // Initialize the ServiceClient (implements IOrganizationService)
        ServiceClient serviceClient = new ServiceClient(connectionString);

        IOrganizationService service = serviceClient;  // use the interface for operations

        var response = (WhoAmIResponse)service.Execute(new WhoAmIRequest());

        Console.WriteLine($"User ID is {response.UserId}.");

        // 2. **Create a new record (CRUD - Create)**  
        // Create an Account record with a name
        Entity account = new Entity("account");
        account["name"] = "Sample Account (SDK Demo)";
        Guid accountId = service.Create(account);
        Console.WriteLine($"Created Account with ID: {accountId}");

        // 3. **Retrieve the record (CRUD - Read)**  
        // Retrieve the account by ID, requesting specific columns
        ColumnSet cols = new ColumnSet("name", "accountid");
        Entity retrieved = service.Retrieve("account", accountId, cols);
        string name = retrieved.GetAttributeValue<string>("name");
        Console.WriteLine($"Retrieved Account Name: {name}");

        // 4. **Update the record (CRUD - Update)**  
        // Modify a field and update the entity
        Entity updateEntity = new Entity("account", accountId);
        updateEntity["name"] = "Updated Account Name";
        service.Update(updateEntity);
        Console.WriteLine("Account name updated.");

        // 5. **Query multiple records**  
        // Query all Account records whose name starts with "Sample"
        QueryExpression query = new QueryExpression("account");
        query.ColumnSet = new ColumnSet("name", "accountid");
        query.Criteria.AddCondition("name", ConditionOperator.BeginsWith, "Sample");
        EntityCollection results = service.RetrieveMultiple(query);
        Console.WriteLine($"Found {results.Entities.Count} account(s) starting with 'Sample'.");
        // (You could iterate results.Entities here. Alternatively, you could use FetchXML or LINQ queries if using early-bound classes.)

        // 6. **Lookup Operations Demo**
        Console.WriteLine("\n=== LOOKUP OPERATIONS DEMO ===");
        DemonstrateLookupOperations(service, accountId);

        // 7. **Delete the record (CRUD - Delete)**  
        service.Delete("account", accountId);
        Console.WriteLine("Account deleted.");

        // // 8. **Trigger a workflow**  
        // // If there is an on-demand workflow (or a custom action) in Dataverse you want to execute:
        // Guid workflowId = new Guid("<WORKFLOW_GUID>");   // ID of the workflow (GUID from your Dataverse environment)
        // var wfRequest = new ExecuteWorkflowRequest
        // {
        //     WorkflowId = workflowId,
        //     EntityId = accountId  // the target record on which to execute the workflow
        // };
        // var wfResponse = (ExecuteWorkflowResponse)service.Execute(wfRequest);
        // Console.WriteLine("Workflow executed. Async Job Id: " + wfResponse.Id);

    }

    /// <summary>
    /// Demonstrates comprehensive lookup operations including push (create/update) and pull (retrieve/query) scenarios
    /// </summary>
    private static void DemonstrateLookupOperations(IOrganizationService service, Guid parentAccountId)
    {
        Console.WriteLine("Starting lookup operations demonstration...");

        // *** PUSH OPERATIONS WITH LOOKUPS ***
        
        // 1. Create a Contact with Account lookup (Parent Customer)
        Guid contactId = CreateContactWithAccountLookup(service, parentAccountId);
        
        // 2. Create another Account to demonstrate lookup updates
        Guid secondAccountId = CreateSecondaryAccount(service);
        
        // 3. Update the Contact's lookup to point to the new Account
        UpdateContactLookup(service, contactId, secondAccountId);
        
        // *** PULL OPERATIONS WITH LOOKUPS ***
        
        // 4. Retrieve Contact with expanded lookup data
        RetrieveContactWithExpandedLookup(service, contactId);
        
        // 5. Query Contacts by Account using lookup filters
        QueryContactsByAccount(service, secondAccountId);
        
        // 6. Query with joins to get related data
        QueryWithJoinsAcrossLookups(service);
        
        // 7. Demonstrate lookup resolution and validation
        DemonstrateLookupValidation(service);
        
        // Cleanup - Delete the created records
        Console.WriteLine("\nCleaning up lookup demo records...");
        try { service.Delete("contact", contactId); Console.WriteLine("Contact deleted."); } catch { }
        try { service.Delete("account", secondAccountId); Console.WriteLine("Secondary account deleted."); } catch { }
    }

    /// <summary>
    /// Creates a Contact record with an Account lookup relationship
    /// </summary>
    private static Guid CreateContactWithAccountLookup(IOrganizationService service, Guid accountId)
    {
        Console.WriteLine("\n1. Creating Contact with Account lookup...");
        
        Entity contact = new Entity("contact");
        contact["firstname"] = "John";
        contact["lastname"] = "Doe";
        contact["emailaddress1"] = "john.doe@example.com";
        
        // Set the Account lookup (Parent Customer field)
        // This is how you create a lookup relationship in Dataverse
        contact["parentcustomerid"] = new EntityReference("account", accountId);
        
        Guid contactId = service.Create(contact);
        Console.WriteLine($"Created Contact with ID: {contactId}");
        Console.WriteLine($"Contact linked to Account ID: {accountId}");
        
        return contactId;
    }

    /// <summary>
    /// Creates a secondary Account for demonstrating lookup updates
    /// </summary>
    private static Guid CreateSecondaryAccount(IOrganizationService service)
    {
        Console.WriteLine("\n2. Creating secondary Account for lookup update demo...");
        
        Entity account = new Entity("account");
        account["name"] = "Secondary Account (Lookup Demo)";
        account["accountcategorycode"] = new OptionSetValue(1); // Preferred Customer
        
        Guid accountId = service.Create(account);
        Console.WriteLine($"Created secondary Account with ID: {accountId}");
        
        return accountId;
    }

    /// <summary>
    /// Updates a Contact's Account lookup to point to a different Account
    /// </summary>
    private static void UpdateContactLookup(IOrganizationService service, Guid contactId, Guid newAccountId)
    {
        Console.WriteLine("\n3. Updating Contact's Account lookup...");
        
        Entity updateContact = new Entity("contact", contactId);
        
        // Update the lookup to point to the new Account
        updateContact["parentcustomerid"] = new EntityReference("account", newAccountId);
        
        service.Update(updateContact);
        Console.WriteLine($"Updated Contact lookup to point to Account ID: {newAccountId}");
    }

    /// <summary>
    /// Retrieves a Contact with expanded lookup data to show the related Account information
    /// </summary>
    private static void RetrieveContactWithExpandedLookup(IOrganizationService service, Guid contactId)
    {
        Console.WriteLine("\n4. Retrieving Contact with expanded lookup data...");
        
        // Method 1: Retrieve with specific columns including lookup fields
        ColumnSet contactColumns = new ColumnSet("firstname", "lastname", "parentcustomerid");
        Entity contact = service.Retrieve("contact", contactId, contactColumns);
        
        Console.WriteLine($"Contact: {contact.GetAttributeValue<string>("firstname")} {contact.GetAttributeValue<string>("lastname")}");
        
        // Get the lookup reference
        EntityReference accountRef = contact.GetAttributeValue<EntityReference>("parentcustomerid");
        if (accountRef != null)
        {
            Console.WriteLine($"Linked to Account ID: {accountRef.Id}");
            Console.WriteLine($"Account Name from reference: {accountRef.Name}");
            
            // To get full Account details, we need a separate query
            Entity linkedAccount = service.Retrieve("account", accountRef.Id, new ColumnSet("name", "accountcategorycode"));
            Console.WriteLine($"Full Account Name: {linkedAccount.GetAttributeValue<string>("name")}");
        }
        
        // Method 2: Using QueryExpression with LinkEntity for joined data
        QueryExpression joinQuery = new QueryExpression("contact");
        joinQuery.ColumnSet = new ColumnSet("firstname", "lastname");
        joinQuery.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
        
        // Add link to Account to get Account data in the same query
        LinkEntity accountLink = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.LeftOuter);
        accountLink.Columns = new ColumnSet("name", "accountcategorycode");
        accountLink.EntityAlias = "parentaccount";
        joinQuery.LinkEntities.Add(accountLink);
        
        EntityCollection joinResults = service.RetrieveMultiple(joinQuery);
        if (joinResults.Entities.Count > 0)
        {
            Entity joinedContact = joinResults.Entities[0];
            Console.WriteLine("Using joined query:");
            Console.WriteLine($"Contact: {joinedContact.GetAttributeValue<string>("firstname")} {joinedContact.GetAttributeValue<string>("lastname")}");
            
            // Access linked Account data using alias
            if (joinedContact.Contains("parentaccount.name"))
            {
                string? accountName = joinedContact.GetAttributeValue<AliasedValue>("parentaccount.name")?.Value?.ToString();
                if (accountName != null)
                {
                    Console.WriteLine($"Parent Account Name: {accountName}");
                }
            }
        }
    }

    /// <summary>
    /// Queries Contacts that belong to a specific Account using lookup filters
    /// </summary>
    private static void QueryContactsByAccount(IOrganizationService service, Guid accountId)
    {
        Console.WriteLine("\n5. Querying Contacts by Account lookup...");
        
        QueryExpression query = new QueryExpression("contact");
        query.ColumnSet = new ColumnSet("firstname", "lastname", "emailaddress1");
        
        // Filter by the Account lookup field
        query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountId);
        
        EntityCollection results = service.RetrieveMultiple(query);
        Console.WriteLine($"Found {results.Entities.Count} contact(s) linked to Account ID: {accountId}");
        
        foreach (Entity contact in results.Entities)
        {
            string firstName = contact.GetAttributeValue<string>("firstname");
            string lastName = contact.GetAttributeValue<string>("lastname");
            string email = contact.GetAttributeValue<string>("emailaddress1");
            Console.WriteLine($"  - {firstName} {lastName} ({email})");
        }
    }

    /// <summary>
    /// Demonstrates complex queries with joins across multiple lookup relationships
    /// </summary>
    private static void QueryWithJoinsAcrossLookups(IOrganizationService service)
    {
        Console.WriteLine("\n6. Querying with joins across lookup relationships...");
        
        // Query Contacts with their Account information and any related Opportunities
        QueryExpression complexQuery = new QueryExpression("contact");
        complexQuery.ColumnSet = new ColumnSet("firstname", "lastname", "emailaddress1");
        
        // Join to Account
        LinkEntity accountLink = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.LeftOuter);
        accountLink.Columns = new ColumnSet("name", "industrycode");
        accountLink.EntityAlias = "account";
        
        // Join from Account to Opportunities (Account can have multiple Opportunities)
        LinkEntity opportunityLink = new LinkEntity("account", "opportunity", "accountid", "customerid", JoinOperator.LeftOuter);
        opportunityLink.Columns = new ColumnSet("name", "estimatedvalue", "statecode");
        opportunityLink.EntityAlias = "opportunity";
        accountLink.LinkEntities.Add(opportunityLink);
        
        complexQuery.LinkEntities.Add(accountLink);
        
        // Add some filtering
        complexQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0); // Active contacts only
        
        try
        {
            EntityCollection complexResults = service.RetrieveMultiple(complexQuery);
            Console.WriteLine($"Found {complexResults.Entities.Count} contact(s) with account and opportunity data:");
            
            foreach (Entity contact in complexResults.Entities)
            {
                string firstName = contact.GetAttributeValue<string>("firstname");
                string lastName = contact.GetAttributeValue<string>("lastname");
                Console.WriteLine($"  Contact: {firstName} {lastName}");
                
                if (contact.Contains("account.name"))
                {
                    string? accountName = contact.GetAttributeValue<AliasedValue>("account.name")?.Value?.ToString();
                    if (accountName != null)
                    {
                        Console.WriteLine($"    Account: {accountName}");
                    }
                }
                
                if (contact.Contains("opportunity.name"))
                {
                    string? opportunityName = contact.GetAttributeValue<AliasedValue>("opportunity.name")?.Value?.ToString();
                    if (opportunityName != null)
                    {
                        Console.WriteLine($"    Opportunity: {opportunityName}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Complex query failed (this is expected in demo environment): {ex.Message}");
        }
    }

    /// <summary>
    /// Demonstrates lookup validation and error handling scenarios
    /// </summary>
    private static void DemonstrateLookupValidation(IOrganizationService service)
    {
        Console.WriteLine("\n7. Demonstrating lookup validation and error handling...");
        
        // Test 1: Try to create Contact with invalid Account lookup
        Console.WriteLine("Testing invalid lookup reference...");
        try
        {
            Entity invalidContact = new Entity("contact");
            invalidContact["firstname"] = "Invalid";
            invalidContact["lastname"] = "Contact";
            
            // Use a non-existent GUID
            Guid nonExistentGuid = Guid.NewGuid();
            invalidContact["parentcustomerid"] = new EntityReference("account", nonExistentGuid);
            
            service.Create(invalidContact);
            Console.WriteLine("ERROR: Should have failed with invalid lookup!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✓ Correctly caught invalid lookup error: {ex.Message}");
        }
        
        // Test 2: Demonstrate clearing a lookup field
        Console.WriteLine("Testing lookup field clearing...");
        try
        {
            Entity testContact = new Entity("contact");
            testContact["firstname"] = "Test";
            testContact["lastname"] = "Clearable";
            Guid testContactId = service.Create(testContact);
            
            // Clear the lookup by setting it to null
            Entity clearLookup = new Entity("contact", testContactId);
            clearLookup["parentcustomerid"] = null;
            service.Update(clearLookup);
            
            Console.WriteLine("✓ Successfully cleared lookup field");
            
            // Cleanup
            service.Delete("contact", testContactId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in lookup clearing test: {ex.Message}");
        }
        
        // Test 3: Demonstrate EntityReference validation
        Console.WriteLine("Testing EntityReference validation...");
        try
        {
            // Show how to validate EntityReference before using it
            EntityReference testRef = new EntityReference("account", Guid.NewGuid());
            
            if (IsValidEntityReference(service, testRef))
            {
                Console.WriteLine("EntityReference is valid");
            }
            else
            {
                Console.WriteLine("✓ EntityReference validation correctly identified invalid reference");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"EntityReference validation test error: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to validate if an EntityReference points to an existing record
    /// </summary>
    private static bool IsValidEntityReference(IOrganizationService service, EntityReference entityRef)
    {
        if (entityRef == null || entityRef.Id == Guid.Empty)
            return false;
        
        try
        {
            // Try to retrieve the record with minimal columns
            service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(false));
            return true;
        }
        catch
        {
            return false;
        }
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