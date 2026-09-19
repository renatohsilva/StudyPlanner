namespace StudyPlanner.Application.Materials.Queries.GetMaterialTopics;

public record MaterialTopicResultDto(Guid TopicId, string TopicName, Guid SubjectId, string SubjectName, decimal RelevanceScore);
