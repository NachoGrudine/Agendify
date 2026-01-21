﻿﻿using Agendify.API.Domain.Entities;

namespace Agendify.API.Services.Auth;

public interface IJwtService
{
    string GenerateToken(User user);
    int? ValidateToken(string token);
}

