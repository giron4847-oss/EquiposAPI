using System.Text;
using System.Text.Json;

namespace EquiposMVC.Infrastructure;

public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _http;

    public ApiClient(IHttpClientFactory factory) => _http = factory.CreateClient("EquiposApi");

    public async Task<T?> GetAsync<T>(string url)
    {
        var resp = await _http.GetAsync(url);
        await Validar(resp);
        return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), Json);
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        var resp = await _http.PostAsync(url, Contenido(body));
        await Validar(resp);
        return JsonSerializer.Deserialize<T>(await resp.Content.ReadAsStringAsync(), Json);
    }

    public async Task PutAsync(string url, object? body = null)
    {
        var resp = await _http.PutAsync(url, Contenido(body));
        await Validar(resp);
    }

    public async Task DeleteAsync(string url)
    {
        var resp = await _http.DeleteAsync(url);
        await Validar(resp);
    }

    private static StringContent Contenido(object? body) =>
        new(body == null ? "" : JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    private static async Task Validar(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode) return;

        var mensaje = (await resp.Content.ReadAsStringAsync()).Trim('"');
        if (mensaje.StartsWith("{") || string.IsNullOrWhiteSpace(mensaje))
            mensaje = "La solicitud no es válida. Revisa los datos.";

        throw new ApiException((int)resp.StatusCode, mensaje);
    }
}
