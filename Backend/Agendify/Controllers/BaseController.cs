using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agendify.Controllers;

/// <summary>
/// Clase base para todos los controllers que requieren acceso al BusinessId del usuario autenticado
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Obtiene el BusinessId del usuario autenticado desde los claims del JWT
    /// </summary>
    /// <returns>El ID del negocio al que pertenece el usuario</returns>
    protected int GetBusinessId()
    {
        return int.Parse(User.FindFirst("BusinessId")?.Value ?? "0");
    }

    /// <summary>
    /// Obtiene el UserId del usuario autenticado desde los claims del JWT
    /// </summary>
    /// <returns>El ID del usuario autenticado</returns>
    protected int GetUserId()
    {
        return int.Parse(User.FindFirst("UserId")?.Value ?? "0");
    }

    /// <summary>
    /// Obtiene el email del usuario autenticado desde los claims del JWT
    /// </summary>
    /// <returns>El email del usuario autenticado</returns>
    protected string? GetUserEmail()
    {
        return User.FindFirst("Email")?.Value;
    }
}

