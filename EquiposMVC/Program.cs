using EquiposMVC.Infrastructure;
using EquiposMVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o => o.Filters.Add<ApiExceptionFilter>());
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(30);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddTransient<JwtDelegatingHandler>();
builder.Services.AddHttpClient("EquiposApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
})
.AddHttpMessageHandler<JwtDelegatingHandler>();

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<PrestamosService>();
builder.Services.AddScoped<EquiposService>();
builder.Services.AddScoped<CategoriasService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
