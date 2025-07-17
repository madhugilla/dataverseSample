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

        // 6. **Delete the record (CRUD - Delete)**  
        service.Delete("account", accountId);
        Console.WriteLine("Account deleted.");

        // // 7. **Trigger a workflow**  
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