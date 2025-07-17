# Dataverse CRUD Operations Sample

This is a .NET console application that demonstrates basic CRUD (Create, Read, Update, Delete) operations with Microsoft Dataverse using the PowerPlatform.Dataverse.Client SDK.

## Features

- **Authentication**: Uses client secret authentication with Azure AD
- **CRUD Operations**: Demonstrates creating, reading, updating, and deleting Account records
- **Query Operations**: Shows how to query multiple records using QueryExpression
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
6. **Deletes** the created record

## Azure AD App Registration

To use this sample, you'll need to register an application in Azure AD with the following permissions:

### Required API Permissions:

1. **Dynamics CRM**
   - **user_impersonation** (Delegated permission)
   - Description: Access Common Data Service as organization users

2. **Microsoft Graph** 
   - **User.Read** (Delegated permission)
   - Description: Sign in and read user profile

## Dependencies

- **Microsoft.PowerPlatform.Dataverse.Client** - Main SDK for Dataverse operations
- **Microsoft.Extensions.Configuration.UserSecrets** - For secure configuration management

## License

This project is provided as a sample for educational purposes.
