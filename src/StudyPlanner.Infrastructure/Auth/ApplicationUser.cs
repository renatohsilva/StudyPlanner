using Microsoft.AspNetCore.Identity;

namespace StudyPlanner.Infrastructure.Auth;

/// <summary>
/// Único registro de usuário do sistema (Identity + perfil). A entidade Domain.User separada foi
/// removida por nunca ser usada — evita manter duas tabelas de "usuário" redundantes.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public required string Name { get; set; }
}
