using Microsoft.Xrm.Sdk;

namespace DataVerseCRUDOperations.Entities;

/// <summary>
/// Early bound entity class representing the Contact entity in Dataverse.
/// </summary>
public class Contact : DataverseEntityBase
{
    /// <summary>
    /// The logical name of the Contact entity.
    /// </summary>
    public override string LogicalName => "contact";
    
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string? LastName { get; set; }
    
    /// <summary>
    /// Gets or sets the full name (computed field).
    /// </summary>
    public string? FullName { get; set; }
    
    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? EmailAddress1 { get; set; }
    
    /// <summary>
    /// Gets or sets the main phone number.
    /// </summary>
    public string? Telephone1 { get; set; }
    
    /// <summary>
    /// Gets or sets the mobile phone number.
    /// </summary>
    public string? MobilePhone { get; set; }
    
    /// <summary>
    /// Gets or sets the job title.
    /// </summary>
    public string? JobTitle { get; set; }
    
    /// <summary>
    /// Gets or sets the parent account ID.
    /// </summary>
    public Guid? ParentCustomerId { get; set; }
    
    /// <summary>
    /// Gets or sets the created on date.
    /// </summary>
    public DateTime? CreatedOn { get; set; }
    
    /// <summary>
    /// Gets or sets the modified on date.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }
    
    /// <summary>
    /// Populates the Entity object with Contact-specific attributes.
    /// </summary>
    /// <param name="entity">The Entity to populate.</param>
    protected override void PopulateEntity(Entity entity)
    {
        if (!string.IsNullOrEmpty(FirstName))
            entity["firstname"] = FirstName;
            
        if (!string.IsNullOrEmpty(LastName))
            entity["lastname"] = LastName;
            
        if (!string.IsNullOrEmpty(EmailAddress1))
            entity["emailaddress1"] = EmailAddress1;
            
        if (!string.IsNullOrEmpty(Telephone1))
            entity["telephone1"] = Telephone1;
            
        if (!string.IsNullOrEmpty(MobilePhone))
            entity["mobilephone"] = MobilePhone;
            
        if (!string.IsNullOrEmpty(JobTitle))
            entity["jobtitle"] = JobTitle;
            
        if (ParentCustomerId.HasValue)
            entity["parentcustomerid"] = new EntityReference("account", ParentCustomerId.Value);
    }
    
    /// <summary>
    /// Populates the Contact entity from Entity attributes.
    /// </summary>
    /// <param name="entity">The source Entity to populate from.</param>
    protected override void PopulateFromEntity(Entity entity)
    {
        FirstName = entity.GetAttributeValue<string>("firstname");
        LastName = entity.GetAttributeValue<string>("lastname");
        FullName = entity.GetAttributeValue<string>("fullname");
        EmailAddress1 = entity.GetAttributeValue<string>("emailaddress1");
        Telephone1 = entity.GetAttributeValue<string>("telephone1");
        MobilePhone = entity.GetAttributeValue<string>("mobilephone");
        JobTitle = entity.GetAttributeValue<string>("jobtitle");
        
        var parentCustomer = entity.GetAttributeValue<EntityReference>("parentcustomerid");
        ParentCustomerId = parentCustomer?.Id;
        
        CreatedOn = entity.GetAttributeValue<DateTime?>("createdon");
        ModifiedOn = entity.GetAttributeValue<DateTime?>("modifiedon");
    }
}