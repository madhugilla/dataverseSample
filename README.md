# Dataverse CRUD Operations Sample

This is a .NET console application that demonstrates basic CRUD (Create, Read, Update, Delete) operations with Microsoft Dataverse using the PowerPlatform.Dataverse.Client SDK.

## Features

- **Authentication**: Uses client secret authentication with Azure AD
- **CRUD Operations**: Demonstrates creating, reading, updating, and deleting Account records
- **Query Operations**: Shows how to query multiple records using QueryExpression
- **Lookup Operations**: Comprehensive demonstration of handling lookup relationships:
  - **Push Operations**: Creating and updating records with lookup references
  - **Pull Operations**: Retrieving and querying data across related entities
  - **Advanced Scenarios**: Complex joins, lookup validation, and error handling
- **Configuration**: Uses .NET User Secrets to securely store connection information

## Prerequisites

- .NET 9.0 or later
- Access to a Microsoft Dataverse environment
- Azure AD app registration with appropriate permissions

## Setup

1. **Clone the repository**
   ```bash
   git clone <your-repo-url>
   cd dataverseSample
   ```

2. **Configure User Secrets**
   
   Set up your Dataverse connection information using the .NET user secrets tool:
   ```bash
   cd DataVerseCRUDOperations
   dotnet user-secrets set "Dataverse:Url" "https://yourorg.crm.dynamics.com"
   dotnet user-secrets set "Dataverse:ClientId" "your-client-id"
   dotnet user-secrets set "Dataverse:ClientSecret" "your-client-secret"
   ```

3. **Restore packages and build**
   ```bash
   dotnet restore
   dotnet build
   ```

## Running the Application

```bash
cd DataVerseCRUDOperations
dotnet run
```

## What the Application Does

1. **Connects to Dataverse** using client secret authentication
2. **Creates** a new Account record
3. **Retrieves** the created record by ID
4. **Updates** the record name
5. **Queries** for multiple records using criteria
6. **Demonstrates Lookup Operations** including:
   - Creating records with lookup relationships (Contact → Account)
   - Updating lookup fields to point to different records
   - Retrieving records with expanded lookup data
   - Querying records using lookup filters
   - Complex queries with joins across multiple related entities
   - Lookup validation and error handling
7. **Deletes** the created records

## Azure AD App Registration

To use this sample, you'll need to register an application in Azure AD with the following permissions:

### Required API Permissions:

1. **Dynamics CRM**
   - **user_impersonation** (Delegated permission)
   - Description: Access Common Data Service as organization users

2. **Microsoft Graph** 
   - **User.Read** (Delegated permission)
   - Description: Sign in and read user profile

### Configure Application User in PowerApps Admin Center

After creating the Azure AD app registration, you need to add it as an application user in your Dataverse environment:

1. **Go to PowerApps Admin Center**
   - Navigate to [PowerApps Admin Center](https://admin.powerplatform.microsoft.com/)

2. **Select Your Environment**
   - Choose the environment where you want to use the application

3. **Access User Settings**
   - In the environment details, go to **Settings**
   - Navigate to **Users + permissions**
   - Select **Application users**

4. **Create New Application User**
   - Click the **+ New app user** button
   - Click **+ Add an app** button
   - Select the app registration you created earlier
   - Assign appropriate security roles (e.g., System Administrator for testing)
   - Click **Create**

> **Note**: The application user needs appropriate security roles to perform CRUD operations on the entities you want to work with.

## Dependencies

- **Microsoft.PowerPlatform.Dataverse.Client** - Main SDK for Dataverse operations
- **Microsoft.Extensions.Configuration.UserSecrets** - For secure configuration management

## Lookup Operations Details

The application demonstrates comprehensive lookup handling scenarios that are commonly needed when working with Dataverse:

### Push Operations (Creating/Updating with Lookups)

1. **Creating Records with Lookups**
   ```csharp
   Entity contact = new Entity("contact");
   contact["firstname"] = "John";
   contact["lastname"] = "Doe";
   // Set lookup to an Account
   contact["parentcustomerid"] = new EntityReference("account", accountId);
   Guid contactId = service.Create(contact);
   ```

2. **Updating Lookup Fields**
   ```csharp
   Entity updateContact = new Entity("contact", contactId);
   updateContact["parentcustomerid"] = new EntityReference("account", newAccountId);
   service.Update(updateContact);
   ```

### Pull Operations (Retrieving/Querying with Lookups)

1. **Retrieving with Lookup References**
   ```csharp
   Entity contact = service.Retrieve("contact", contactId, new ColumnSet("firstname", "parentcustomerid"));
   EntityReference accountRef = contact.GetAttributeValue<EntityReference>("parentcustomerid");
   ```

2. **Querying with Joined Data**
   ```csharp
   QueryExpression query = new QueryExpression("contact");
   LinkEntity accountLink = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.LeftOuter);
   accountLink.EntityAlias = "parentaccount";
   query.LinkEntities.Add(accountLink);
   ```

3. **Filtering by Lookup Values**
   ```csharp
   query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountId);
   ```

### Advanced Scenarios

- **Complex Multi-Entity Joins**: Demonstrates querying across Contact → Account → Opportunity relationships
- **Lookup Validation**: Shows how to validate EntityReference objects before use
- **Error Handling**: Proper handling of invalid lookup references
- **Clearing Lookups**: How to set lookup fields to null/empty

These patterns can be applied to any lookup relationship in Dataverse, making this a comprehensive reference for handling related data operations.

## License

This project is provided as a sample for educational purposes.
