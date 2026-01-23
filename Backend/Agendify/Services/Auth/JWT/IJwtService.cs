using Agendify.Models.Entities;

namespace Agendify.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}

