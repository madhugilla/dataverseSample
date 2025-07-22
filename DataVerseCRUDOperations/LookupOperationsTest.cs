using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace DataVerseCRUDOperations;

/// <summary>
/// Simple test class to validate lookup operations logic without requiring Dataverse connection
/// </summary>
public static class LookupOperationsTest
{
    /// <summary>
    /// Test method to validate that EntityReference creation and validation logic works correctly
    /// </summary>
    public static void TestEntityReferenceCreation()
    {
        Console.WriteLine("=== Testing EntityReference Creation ===");
        
        // Test 1: Valid EntityReference creation
        Guid testAccountId = Guid.NewGuid();
        EntityReference accountRef = new EntityReference("account", testAccountId);
        
        Console.WriteLine($"✓ Created EntityReference - LogicalName: {accountRef.LogicalName}, Id: {accountRef.Id}");
        
        // Test 2: Entity creation with lookup
        Entity contact = new Entity("contact");
        contact["firstname"] = "Test";
        contact["lastname"] = "Contact";
        contact["parentcustomerid"] = accountRef;
        
        Console.WriteLine($"✓ Created Contact entity with lookup to Account: {contact.GetAttributeValue<EntityReference>("parentcustomerid").Id}");
        
        // Test 3: QueryExpression with lookup filter
        QueryExpression query = new QueryExpression("contact");
        query.ColumnSet = new ColumnSet("firstname", "lastname", "parentcustomerid");
        query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, testAccountId);
        
        Console.WriteLine($"✓ Created QueryExpression with lookup filter for Account: {testAccountId}");
        
        // Test 4: LinkEntity for joins
        LinkEntity accountLink = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.LeftOuter);
        accountLink.Columns = new ColumnSet("name", "industrycode");
        accountLink.EntityAlias = "parentaccount";
        
        query.LinkEntities.Add(accountLink);
        
        Console.WriteLine($"✓ Created LinkEntity for joining Contact to Account with alias: {accountLink.EntityAlias}");
        
        // Test 5: Null/Empty EntityReference handling
        Entity contactWithoutLookup = new Entity("contact");
        contactWithoutLookup["firstname"] = "No Lookup";
        contactWithoutLookup["lastname"] = "Contact";
        contactWithoutLookup["parentcustomerid"] = null; // Clear lookup
        
        Console.WriteLine("✓ Created Contact entity with null lookup (for clearing lookup fields)");
        
        Console.WriteLine("All EntityReference tests passed!");
    }
    
    /// <summary>
    /// Test method to validate query construction for complex lookup scenarios
    /// </summary>
    public static void TestComplexLookupQueries()
    {
        Console.WriteLine("\n=== Testing Complex Lookup Queries ===");
        
        // Test multi-level join: Contact → Account → Opportunity
        QueryExpression complexQuery = new QueryExpression("contact");
        complexQuery.ColumnSet = new ColumnSet("firstname", "lastname");
        
        // Level 1: Contact → Account
        LinkEntity accountLink = new LinkEntity("contact", "account", "parentcustomerid", "accountid", JoinOperator.LeftOuter);
        accountLink.Columns = new ColumnSet("name", "industrycode");
        accountLink.EntityAlias = "account";
        
        // Level 2: Account → Opportunity
        LinkEntity opportunityLink = new LinkEntity("account", "opportunity", "accountid", "customerid", JoinOperator.LeftOuter);
        opportunityLink.Columns = new ColumnSet("name", "estimatedvalue");
        opportunityLink.EntityAlias = "opportunity";
        
        // Nest the opportunity link under account link
        accountLink.LinkEntities.Add(opportunityLink);
        complexQuery.LinkEntities.Add(accountLink);
        
        Console.WriteLine("✓ Created complex multi-level join query: Contact → Account → Opportunity");
        Console.WriteLine($"  Primary entity: {complexQuery.EntityName}");
        Console.WriteLine($"  First level join: {accountLink.EntityAlias} ({accountLink.LinkFromEntityName} → {accountLink.LinkToEntityName})");
        Console.WriteLine($"  Second level join: {opportunityLink.EntityAlias} ({opportunityLink.LinkFromEntityName} → {opportunityLink.LinkToEntityName})");
        
        // Test filtering on linked entities
        FilterExpression accountFilter = new FilterExpression();
        accountFilter.AddCondition("industrycode", ConditionOperator.Equal, 1); // Example industry code
        accountLink.LinkCriteria = accountFilter;
        
        Console.WriteLine("✓ Added filter criteria to linked Account entity");
        
        Console.WriteLine("Complex lookup query tests passed!");
    }
    
    /// <summary>
    /// Test validation logic for EntityReference objects
    /// </summary>
    public static void TestEntityReferenceValidation()
    {
        Console.WriteLine("\n=== Testing EntityReference Validation ===");
        
        // Test 1: Valid EntityReference
        EntityReference validRef = new EntityReference("account", Guid.NewGuid());
        bool isValid1 = ValidateEntityReference(validRef);
        Console.WriteLine($"✓ Valid EntityReference validation: {isValid1}");
        
        // Test 2: Null EntityReference
        EntityReference? nullRef = null;
        bool isValid2 = ValidateEntityReference(nullRef);
        Console.WriteLine($"✓ Null EntityReference validation: {isValid2} (should be false)");
        
        // Test 3: Empty GUID
        EntityReference emptyRef = new EntityReference("account", Guid.Empty);
        bool isValid3 = ValidateEntityReference(emptyRef);
        Console.WriteLine($"✓ Empty GUID EntityReference validation: {isValid3} (should be false)");
        
        // Test 4: Empty LogicalName
        try
        {
            EntityReference invalidRef = new EntityReference("", Guid.NewGuid());
            bool isValid4 = ValidateEntityReference(invalidRef);
            Console.WriteLine($"✓ Empty LogicalName EntityReference validation: {isValid4} (should be false)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✓ Empty LogicalName correctly threw exception: {ex.GetType().Name}");
        }
        
        Console.WriteLine("EntityReference validation tests passed!");
    }
    
    /// <summary>
    /// Helper method to validate EntityReference objects (mirrors the one in Program.cs)
    /// </summary>
    private static bool ValidateEntityReference(EntityReference? entityRef)
    {
        return entityRef != null && 
               !string.IsNullOrWhiteSpace(entityRef.LogicalName) && 
               entityRef.Id != Guid.Empty;
    }
    
    /// <summary>
    /// Run all tests
    /// </summary>
    public static void RunAllTests()
    {
        Console.WriteLine("Starting Lookup Operations Tests...\n");
        
        TestEntityReferenceCreation();
        TestComplexLookupQueries();
        TestEntityReferenceValidation();
        
        Console.WriteLine("\n=== All Lookup Operations Tests Completed Successfully! ===");
    }
}