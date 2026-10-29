using EquiposMVC.Infrastructure;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public class EquiposService
{
    private readonly ApiClient _api;
    public EquiposService(ApiClient api) => _api = api;

    public async Task<List<EquipoListItem>> ListarAsync(string? texto, int? categoriaId, string? estado)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(texto)) qs.Add($"texto={Uri.EscapeDataString(texto)}");
        if (categoriaId.HasValue) qs.Add($"categoriaId={categoriaId}");
        if (!string.IsNullOrWhiteSpace(estado)) qs.Add($"estado={estado}");

        var url = "api/Equipos" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        return await _api.GetAsync<List<EquipoListItem>>(url) ?? new();
    }

    public Task<EquipoDetalle?> ObtenerAsync(int id) => _api.GetAsync<EquipoDetalle>($"api/Equipos/{id}");

    public Task CrearAsync(EquipoFormViewModel m) =>
        _api.PostAsync<object>("api/Equipos", new { m.CodigoInventario, m.Nombre, m.Descripcion, m.CategoriaId });

    public Task ActualizarAsync(EquipoFormViewModel m) =>
        _api.PutAsync($"api/Equipos/{m.Id}", new { m.CodigoInventario, m.Nombre, m.Descripcion, m.CategoriaId, m.Estado, m.RowVersion });

    public Task EliminarAsync(int id) => _api.DeleteAsync($"api/Equipos/{id}");
}
