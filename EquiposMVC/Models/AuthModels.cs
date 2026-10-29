using System.ComponentModel.DataAnnotations;

namespace EquiposMVC.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu usuario")] public string Username { get; set; } = string.Empty;
    [Required(ErrorMessage = "Escribe tu contraseña")] public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}