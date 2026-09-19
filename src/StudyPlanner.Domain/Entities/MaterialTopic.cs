namespace StudyPlanner.Domain.Entities;

/// <summary>Relação N:N Material↔Topic com o score de relevância (similaridade de cosseno máxima entre os chunks do material e o tópico).</summary>
public class MaterialTopic
{
    public required Guid MaterialId { get; set; }
    public required Guid TopicId { get; set; }
    public required decimal RelevanceScore { get; set; }
}
