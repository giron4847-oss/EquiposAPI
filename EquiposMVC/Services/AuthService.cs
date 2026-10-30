using EquiposMVC.Infrastructure;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

// Servicio de autenticación: el controlador ya no habla directo con ApiClient
public class AuthService
{
    private readonly ApiClient _api;
    public AuthService(ApiClient api) => _api = api;

    public async Task<LoginResponse> LoginAsync(LoginViewModel model) =>
        (await _api.PostAsync<LoginResponse>("api/Auth/login", model))!;
}
