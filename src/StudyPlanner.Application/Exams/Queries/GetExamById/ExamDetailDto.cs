namespace StudyPlanner.Application.Exams.Queries.GetExamById;

public record ExamDetailDto(
    Guid Id,
    string Name,
    DateOnly ExamDate,
    string Status,
    IReadOnlyList<SubjectDto> Subjects);

public record SubjectDto(
    Guid Id,
    string Name,
    decimal Weight,
    IReadOnlyList<TopicDto> Topics);

public record TopicDto(
    Guid Id,
    Guid? ParentTopicId,
    string Name,
    decimal EstimatedIncidence);
