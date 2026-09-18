using MediatR;
using StudyPlanner.Application.Notices.Models;

namespace StudyPlanner.Application.Notices.Commands.ConfirmNoticeStructure;

/// <summary>
/// Materializa em Subject/Topic a estrutura extraída — possivelmente editada pelo usuário na tela
/// de confirmação. É o único caminho pelo qual a saída do LLM vira dado real do sistema.
/// </summary>
public record ConfirmNoticeStructureCommand(Guid NoticeId, ExtractedExamStructure Structure) : IRequest;
