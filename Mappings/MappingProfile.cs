using AutoMapper;
using EquiposAPI.Dtos;
using EquiposAPI.Models;

namespace EquiposAPI.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Categoria, CategoriaDto>();
        CreateMap<CategoriaCreateDto, Categoria>();
        CreateMap<CategoriaUpdateDto, Categoria>();
        CreateMap<Prestamo, PrestamoDto>();   // EquipoNombre y UsuarioUsername se llenan solos

        // CategoriaNombre se llena solo desde Categoria.Nombre (AutoMapper lo "aplana")
        CreateMap<Equipo, EquipoListDto>();
        CreateMap<Equipo, EquipoDetalleDto>();
        CreateMap<EquipoCreateDto, Equipo>();
        CreateMap<EquipoUpdateDto, Equipo>()
            .ForMember(d => d.RowVersion, o => o.Ignore());   // se maneja a mano para la concurrencia
    }
}
