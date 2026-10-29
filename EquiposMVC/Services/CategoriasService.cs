using EquiposMVC.Infrastructure;
using EquiposMVC.Models;

namespace EquiposMVC.Services;

public class CategoriasService
{
    private readonly ApiClient _api;
    public CategoriasService(ApiClient api) => _api = api;

    public async Task<List<CategoriaItem>> ListarAsync() =>
        await _api.GetAsync<List<CategoriaItem>>("api/Categorias") ?? new();

    public Task<CategoriaItem?> ObtenerAsync(int id) => _api.GetAsync<CategoriaItem>($"api/Categorias/{id}");
    public Task CrearAsync(string nombre) => _api.PostAsync<object>("api/Categorias", new { Nombre = nombre });
    public Task ActualizarAsync(int id, string nombre) => _api.PutAsync($"api/Categorias/{id}", new { Nombre = nombre });
    public Task EliminarAsync(int id) => _api.DeleteAsync($"api/Categorias/{id}");
}
