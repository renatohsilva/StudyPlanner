using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Questions.Commands.CreateQuestion;

public class CreateQuestionHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
        var topicIds = request.TopicIds.Distinct().ToList();

        if (topicIds.Count > 0 && request.CreatedByUserId is { } userId)
        {
            var ownedCount = await (
                from topic in db.Topics
                join subject in db.Subjects on topic.SubjectId equals subject.Id
                join exam in db.Exams on subject.ExamId equals exam.Id
                where topicIds.Contains(topic.Id) && exam.UserId == userId
                select topic.Id).Distinct().CountAsync(cancellationToken);

            if (ownedCount != topicIds.Count) throw new KeyNotFoundException("One or more topics not found.");
        }

        var question = new Question
        {
            Statement = request.Statement,
            AlternativesJson = request.AlternativesJson,
            CorrectAnswer = request.CorrectAnswer,
            Source = request.Source,
            BoardId = request.BoardId,
            Year = request.Year,
            CreatedByUserId = request.CreatedByUserId
        };

        db.Questions.Add(question);

        foreach (var topicId in topicIds)
        {
            db.QuestionTopics.Add(new QuestionTopic { QuestionId = question.Id, TopicId = topicId });
        }

        await db.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
