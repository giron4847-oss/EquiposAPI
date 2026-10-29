using System.Net.Http.Headers;

namespace EquiposMVC.Infrastructure;

public class JwtDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;

    public JwtDelegatingHandler(IHttpContextAccessor accessor) => _accessor = accessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _accessor.HttpContext?.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
