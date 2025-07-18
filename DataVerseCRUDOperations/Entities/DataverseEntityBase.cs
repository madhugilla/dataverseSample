using Microsoft.Xrm.Sdk;

namespace DataVerseCRUDOperations.Entities;

/// <summary>
/// Base class for all early bound Dataverse entities.
/// Provides common implementation for entity operations.
/// </summary>
public abstract class DataverseEntityBase : IDataverseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Gets the logical name of the entity in Dataverse.
    /// </summary>
    public abstract string LogicalName { get; }
    
    /// <summary>
    /// Converts the early bound entity to a late bound Entity object.
    /// </summary>
    /// <returns>Entity object for Dataverse operations.</returns>
    public virtual Entity ToEntity()
    {
        var entity = new Entity(LogicalName);
        if (Id != Guid.Empty)
        {
            entity.Id = Id;
        }
        PopulateEntity(entity);
        return entity;
    }
    
    /// <summary>
    /// Updates the early bound entity from a late bound Entity object.
    /// </summary>
    /// <param name="entity">The source Entity to update from.</param>
    public virtual void FromEntity(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        if (entity.LogicalName != LogicalName)
            throw new ArgumentException($"Entity logical name '{entity.LogicalName}' does not match expected '{LogicalName}'");
            
        Id = entity.Id;
        PopulateFromEntity(entity);
    }
    
    /// <summary>
    /// Override this method to populate the Entity object with specific attributes.
    /// </summary>
    /// <param name="entity">The Entity to populate.</param>
    protected abstract void PopulateEntity(Entity entity);
    
    /// <summary>
    /// Override this method to populate the early bound entity from Entity attributes.
    /// </summary>
    /// <param name="entity">The source Entity to populate from.</param>
    protected abstract void PopulateFromEntity(Entity entity);
}