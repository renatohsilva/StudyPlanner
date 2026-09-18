using StudyPlanner.Domain.Common;

namespace StudyPlanner.Domain.Entities;

public class User : Entity
{
    public required string Email { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
