using Microsoft.Xrm.Sdk;

namespace DataVerseCRUDOperations.Entities;

/// <summary>
/// Base interface for all early bound Dataverse entities.
/// Provides common properties and methods for entity operations.
/// </summary>
public interface IDataverseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }
    
    /// <summary>
    /// Gets the logical name of the entity in Dataverse.
    /// </summary>
    string LogicalName { get; }
    
    /// <summary>
    /// Converts the early bound entity to a late bound Entity object.
    /// </summary>
    /// <returns>Entity object for Dataverse operations.</returns>
    Entity ToEntity();
    
    /// <summary>
    /// Updates the early bound entity from a late bound Entity object.
    /// </summary>
    /// <param name="entity">The source Entity to update from.</param>
    void FromEntity(Entity entity);
}