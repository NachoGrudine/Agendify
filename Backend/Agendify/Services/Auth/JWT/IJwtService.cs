using Agendify.Models.Entities;

namespace Agendify.Services.Auth.JWT;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}

