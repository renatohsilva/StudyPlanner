using MediatR;
using StudyPlanner.Application.Common.Interfaces;
using StudyPlanner.Domain.Entities;

namespace StudyPlanner.Application.Questions.Commands.CreateQuestion;

public class CreateQuestionHandler(IStudyPlannerDbContext db) : IRequestHandler<CreateQuestionCommand, Guid>
{
    public async Task<Guid> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
    {
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

        foreach (var topicId in request.TopicIds.Distinct())
        {
            db.QuestionTopics.Add(new QuestionTopic { QuestionId = question.Id, TopicId = topicId });
        }

        await db.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
