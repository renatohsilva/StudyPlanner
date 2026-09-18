namespace StudyPlanner.Application.Notices.Models;

public record ExtractedExamStructure(IReadOnlyList<ExtractedSubject> Subjects);

public record ExtractedSubject(string Name, decimal Weight, IReadOnlyList<ExtractedTopic> Topics);

public record ExtractedTopic(string Name, decimal EstimatedIncidence);
