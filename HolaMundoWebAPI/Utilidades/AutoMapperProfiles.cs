using System.Globalization;
using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {

        public AutoMapperProfiles()
        {

            CreateMap<LlaveAPI, LlaveDTO>();

            CreateMap<Autor, AutorDTO>()
                .ForMember(dto => dto.NombreCompleto,
                config => config.MapFrom(autor => MapearNombreYApellido(autor)));


            CreateMap<Autor, AutorConLibrosDTO>()
               .ForMember(dto => dto.NombreCompleto,
               config => config.MapFrom(autor => MapearNombreYApellido(autor)));



            CreateMap<AutorCrearFotoDTO, Autor>()
                .ForMember(ent => ent.Foto, config => config.Ignore());
            CreateMap<AutorCrearDTO, Autor>();
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();
            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.id,
                config => config.MapFrom(ent => ent.LibroId))
                .ForMember(dto => dto.Titulo,
                config => config.MapFrom(ent => ent.Libro!.Titulo));



            CreateMap<Libro, LibroDTO>();

            CreateMap<LibroCrearDTO, Libro>().ForMember(ent => ent.Autores,
                config => config.MapFrom(dto => dto.AutoresIds
                .Select(id => new AutorLibro { AutorId = id})));



            CreateMap<Libro, LibroConAutoresDTO>();

            CreateMap<AutorLibro, AutorDTO>()
                .ForMember(dto => dto.Id,
                config => config.MapFrom(ent => ent.AutorId))
                .ForMember(dto => dto.NombreCompleto,
                config => config
                .MapFrom(ent => MapearNombreYApellido(ent.Autor!)));



            CreateMap<LibroCrearDTO, AutorLibro>()
                .ForMember(ent => ent.Libro, config => config
                .MapFrom(dto => new Libro { Titulo = dto.Titulo }));


            CreateMap<ComentarioDTO, Comentario>();
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config
                .MapFrom(ent => ent.Usuario!.Email));
            CreateMap<ComentarioCrearDTO, Comentario>();
            CreateMap<ComentarioPatchDTO, Comentario>().ReverseMap();


            CreateMap<Usuario, UsuarioDTO>();

        }

        private string MapearNombreYApellido(Autor autor) =>  $"{autor.Nombres} {autor.Apellidos}";


    }
}
