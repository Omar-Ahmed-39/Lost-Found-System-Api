using System;
using LostAndFound.Core.Interfaces;

namespace LostAndFound.Core.Entities;

public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
