using EquiposMVC.Infrastructure;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public class PrestamosService
{
    private readonly ApiClient _api;
    public PrestamosService(ApiClient api) => _api = api;

    public async Task<List<PrestamoItem>> ListarAsync(bool soloVencidos) =>
        await _api.GetAsync<List<PrestamoItem>>("api/Prestamos?soloVencidos=" + (soloVencidos ? "true" : "false")) ?? new();

    public Task CrearAsync(PrestamoFormViewModel m) =>
        _api.PostAsync<object>("api/Prestamos", new { m.EquipoId, m.UsuarioId, m.FechaVencimiento, m.Observaciones });

    public Task DevolverAsync(int id) => _api.PutAsync($"api/Prestamos/{id}/devolucion");
}