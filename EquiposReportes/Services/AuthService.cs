using System.Text;
using System.Text.Json;

namespace EquiposReportes.Services;

// Login contra la API: el modulo de reportes NO guarda usuarios ni contrasenas propias
public class AuthService
{
    private readonly IHttpClientFactory _factory;
    public AuthService(IHttpClientFactory factory) => _factory = factory;

    // Devuelve (ok, rol, mensaje)
    public async Task<(bool Ok, string Rol, string Mensaje)> LoginAsync(string username, string password)
    {
        var client = _factory.CreateClient("EquiposApi");
        var body = new StringContent(JsonSerializer.Serialize(new { Username = username, Password = password }),
                                     Encoding.UTF8, "application/json");
        HttpResponseMessage resp;
        try { resp = await client.PostAsync("api/auth/login", body); }
        catch { return (false, "", "No se pudo conectar con la API. ¿Está iniciada?"); }

        if (!resp.IsSuccessStatusCode) return (false, "", "Usuario o contraseña incorrectos.");

        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        string? token = null;
        foreach (var p in doc.RootElement.EnumerateObject())
            if (p.Name.Equals("token", StringComparison.OrdinalIgnoreCase)) token = p.Value.GetString();
        if (token == null) return (false, "", "La API no devolvió un token.");

        return (true, LeerRol(token), "");
    }

    // El rol viene en los claims (payload) del JWT
    private static string LeerRol(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        using var doc = JsonDocument.Parse(Convert.FromBase64String(payload));
        foreach (var p in doc.RootElement.EnumerateObject())
            if (p.Name == "role" || p.Name.EndsWith("/claims/role"))
                return p.Value.ValueKind == JsonValueKind.Array ? p.Value[0].GetString() ?? "" : p.Value.GetString() ?? "";
        return "";
    }
}
