using System.ComponentModel.DataAnnotations;

namespace EquiposReportes.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Usuario obligatorio")] public string Username { get; set; } = "";
    [Required(ErrorMessage = "Contraseña obligatoria")] public string Password { get; set; } = "";
}

public class FiltroReporte
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public int? CategoriaId { get; set; }
    public string? Username { get; set; }
}

public class CategoriaOpcion { public int Id { get; set; } public string Nombre { get; set; } = ""; }

public class InventarioCategoriaVM
{
    public string Categoria { get; set; } = "";
    public int Total { get; set; }
    public int Disponibles { get; set; }
    public int Prestados { get; set; }
    public int Mantenimiento { get; set; }
}

public class PrestamoVencidoVM
{
    public string Username { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Equipo { get; set; } = "";
    public DateTime FechaVencimiento { get; set; }
    public int DiasAtraso { get; set; }
}

public class TopEquipoVM
{
    public string Codigo { get; set; } = "";
    public string Equipo { get; set; } = "";
    public int Prestamos { get; set; }
}

public class HistorialUsuarioVM
{
    public string Username { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Equipo { get; set; } = "";
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaDevolucion { get; set; }
}

// Un ViewModel por pantalla: filtro + categorias + resultados
public class ReporteViewModel<T>
{
    public FiltroReporte Filtro { get; set; } = new();
    public List<CategoriaOpcion> Categorias { get; set; } = new();
    public List<T> Datos { get; set; } = new();
}
