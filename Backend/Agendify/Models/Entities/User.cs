namespace Agendify.Models.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int BusinessId { get; set; }
    
    // Navigation properties
    public Business? Business { get; set; }
}

