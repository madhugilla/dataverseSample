using Microsoft.Xrm.Sdk;

namespace DataVerseCRUDOperations.Entities;

/// <summary>
/// Early bound entity class representing the Account entity in Dataverse.
/// </summary>
public class Account : DataverseEntityBase
{
    /// <summary>
    /// The logical name of the Account entity.
    /// </summary>
    public override string LogicalName => "account";
    
    /// <summary>
    /// Gets or sets the account name.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// Gets or sets the main phone number.
    /// </summary>
    public string? Telephone1 { get; set; }
    
    /// <summary>
    /// Gets or sets the website URL.
    /// </summary>
    public string? WebSiteUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the number of employees.
    /// </summary>
    public int? NumberOfEmployees { get; set; }
    
    /// <summary>
    /// Gets or sets the annual revenue.
    /// </summary>
    public decimal? Revenue { get; set; }
    
    /// <summary>
    /// Gets or sets the created on date.
    /// </summary>
    public DateTime? CreatedOn { get; set; }
    
    /// <summary>
    /// Gets or sets the modified on date.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
    
    /// <summary>
    /// Populates the Entity object with Account-specific attributes.
    /// </summary>
    /// <param name="entity">The Entity to populate.</param>
    protected override void PopulateEntity(Entity entity)
    {
        if (!string.IsNullOrEmpty(Name))
            entity["name"] = Name;
            
        if (!string.IsNullOrEmpty(AccountNumber))
            entity["accountnumber"] = AccountNumber;
            
        if (!string.IsNullOrEmpty(Telephone1))
            entity["telephone1"] = Telephone1;
            
        if (!string.IsNullOrEmpty(WebSiteUrl))
            entity["websiteurl"] = WebSiteUrl;
            
        if (!string.IsNullOrEmpty(Description))
            entity["description"] = Description;
            
        if (NumberOfEmployees.HasValue)
            entity["numberofemployees"] = NumberOfEmployees.Value;
            
        if (Revenue.HasValue)
            entity["revenue"] = new Money(Revenue.Value);
    }
    
    /// <summary>
    /// Populates the Account entity from Entity attributes.
    /// </summary>
    /// <param name="entity">The source Entity to populate from.</param>
    protected override void PopulateFromEntity(Entity entity)
    {
        Name = entity.GetAttributeValue<string>("name");
        AccountNumber = entity.GetAttributeValue<string>("accountnumber");
        Telephone1 = entity.GetAttributeValue<string>("telephone1");
        WebSiteUrl = entity.GetAttributeValue<string>("websiteurl");
        Description = entity.GetAttributeValue<string>("description");
        NumberOfEmployees = entity.GetAttributeValue<int?>("numberofemployees");
        
        var revenueAttribute = entity.GetAttributeValue<Money>("revenue");
        Revenue = revenueAttribute?.Value;
        
        CreatedOn = entity.GetAttributeValue<DateTime?>("createdon");
        ModifiedOn = entity.GetAttributeValue<DateTime?>("modifiedon");
    }
}