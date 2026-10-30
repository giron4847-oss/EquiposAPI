using EquiposReportes.Data;
using EquiposReportes.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(o => { o.IdleTimeout = TimeSpan.FromMinutes(30); o.Cookie.HttpOnly = true; o.Cookie.IsEssential = true; });

// Cadena de conexion (usuario "reportes" con db_datareader) viene de User Secrets
builder.Services.AddDbContext<EquiposDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("ReportesConnection")));

// URL de la API viene de appsettings.json
builder.Services.AddHttpClient("EquiposApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!));
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

app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
app.Run();
