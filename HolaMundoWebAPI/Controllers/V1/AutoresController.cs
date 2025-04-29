using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Migrations;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPI.Utilidades;
using BibliotecaAPI.Utilidades.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
namespace BibliotecaAPI.Controllers.V1
{


    [ApiController]
    [Route("api/v1/autores")]
    [Authorize(Policy = "esadmin")]    public class AutoresController: ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<AutoresController> _logger;
        private readonly IOutputCacheStore _cacheStore;
        private readonly IServicioAutores servicioAutoresV1;
        private const string contenedor = "autores";
        private const string cache = "autores-obtener";

        public AutoresController(ApplicationDbContext context, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos, ILogger<AutoresController> logger,
            IOutputCacheStore cacheStore, IServicioAutores servicioAutoresV1)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _cacheStore = cacheStore;
            this.servicioAutoresV1 = servicioAutoresV1;
            this.almacenadorArchivos = almacenadorArchivos;
            
        }


        [HttpGet(Name = "ObtenerAutoresV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAutoresAttribute>()]
        public async Task<IEnumerable<AutorDTO>>
            Get([FromQuery] PaginacionDTO paginacionDTO)
        {

           return await servicioAutoresV1.Get(paginacionDTO);

            
            
        }



        [HttpGet("{id:int}", Name = "ObtenerAutorV1")]
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por Id")]
        [EndpointDescription("Obtiene autor por id y sus libros, si no existe se devuelve 404")]
        [ProducesResponseType<AutorConLibrosDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOASAutorAttribute>()]
        public async Task<ActionResult<AutorConLibrosDTO>> Get(
            [Description("El id de autor")]int id)
        {
            var autor = await _context.Autores
                .Include(x => x.Libros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null) { 
            
                return NotFound();
            }

            var autorDTO = _mapper.Map<AutorConLibrosDTO>(autor);

            return autorDTO;
        }


        [HttpGet("filtrar", Name = "FiltrarAutoresV1")]
        [AllowAnonymous]
        public async Task<ActionResult> Filtrar([FromQuery] AutorFiltroDTO autorFiltroDTO)
        {
            var queryable = _context.Autores.AsQueryable();

            if (!string.IsNullOrEmpty(autorFiltroDTO.Nombres))
            {
                queryable = queryable.Where(x => x.Nombres.Contains(autorFiltroDTO.Nombres));
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.Apellidos))
            {
                queryable = queryable.Where(x => x.Apellidos.Contains(autorFiltroDTO.Apellidos));
            }

            if (autorFiltroDTO.IncluirLibros)
            {
                queryable = queryable.Include(x => x.Libros).ThenInclude(x => x.Libro);
            }


            if (autorFiltroDTO.TieneFoto.HasValue)
            {
                if (autorFiltroDTO.TieneFoto.Value)
                {
                    queryable = queryable.Where(x => x.Foto != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Foto == null);
                }
            }

            if (autorFiltroDTO.TieneLibros.HasValue)
            {
                if (autorFiltroDTO.TieneLibros.Value)
                {
                    queryable = queryable.Where(x => x.Libros.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Libros.Any());
                }
            }

            if (!string.IsNullOrEmpty(autorFiltroDTO.TituloLibro))
            {
                queryable = queryable.Where(x =>
                    x.Libros.Any(y => y.Libro!.Titulo.Contains(autorFiltroDTO.TituloLibro)));
            }


            if (!string.IsNullOrEmpty(autorFiltroDTO.CampoOrdenar))
            {
                var tipoOrden = autorFiltroDTO.OrdenAscendente ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{autorFiltroDTO.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Nombres);
                    _logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Nombres);
            }

            var autores = await queryable
                    .Paginar(autorFiltroDTO.PaginacionDTO).ToListAsync();
            if (autorFiltroDTO.IncluirLibros)
            {
                var autoresDTO = _mapper.Map<IEnumerable<AutorConLibrosDTO>>(autores);
                return Ok(autoresDTO);
            }
            else
            {
                var autoresDTO = _mapper.Map<IEnumerable<AutorDTO>>(autores);
                return Ok(autoresDTO);
            }

        }


        [HttpPost(Name = "CrearAutorV1")]
         public async Task<ActionResult> Post(AutorCrearDTO autorCrearDTO)
         {          
            var autor = _mapper.Map<Autor>(autorCrearDTO);  
            _context.Add(autor);
            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);
            var autorDTO = _mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("ObtenerAutorV1", new {id = autor.Id}, autorDTO);
         }



        [HttpPost("con-foto", Name = "CrearAutorConFotoV1")]
        public async Task<ActionResult> PostConFoto([FromForm] AutorCrearFotoDTO autorCrearDTO)
        {
            var autor = _mapper.Map<Autor>(autorCrearDTO);
            if (autorCrearDTO.Foto != null) 
            {
                var url = await almacenadorArchivos.Almacenar(contenedor, autorCrearDTO.Foto);
                autor.Foto = url;
            
            }
            
            
            _context.Add(autor);
            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);

            var autorDTO = _mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("ObtenerAutorV1", new { id = autor.Id }, autorDTO);
        }


        [HttpPut("{id:int}", Name = "ActualizarAutorV1")]
        public async Task<ActionResult> Put(int id, [FromForm] AutorCrearFotoDTO autorCrearDTO)
        {
            // Comprobar si el autor existe en la base de datos
            var existeAutor = await _context.Autores.AnyAsync(x => x.Id == id);

            // Si no existe, devolver NotFound
            if (!existeAutor)
            {
                return NotFound();
            }

            // Mapear el DTO al modelo de entidad
            var autor = _mapper.Map<Autor>(autorCrearDTO);
            autor.Id = id; // Establecer el ID del autor

            // Si se ha enviado una nueva foto, procesarla
            if (autorCrearDTO.Foto != null)
            {
                var fotoActual = await _context
                                    .Autores.Where(x => x.Id == id)
                                    .Select(x => x.Foto).FirstOrDefaultAsync();

                // Si ya tiene foto, editarla; si no, almacenarla como nueva
                var url = await almacenadorArchivos.Editar(fotoActual, contenedor, autorCrearDTO.Foto);
                autor.Foto = url;
            }

            // Actualizar el autor en la base de datos
            _context.Update(autor);
            await _context.SaveChangesAsync();

            // Invalidar el caché correspondiente
            await _cacheStore.EvictByTagAsync(cache, default);

            // Retornar NoContent si todo fue bien
            return NoContent();
        }



        [HttpPatch("{id:int}", Name = "PatchAutorV1")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AutorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }

            var autorDb = await _context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if (autorDb is null)
            {
                return NotFound();
            }

            var autorPatch = _mapper.Map<AutorPatchDTO>(autorDb);

            patchDoc.ApplyTo(autorPatch, ModelState);

            var esValido = TryValidateModel(autorPatch);

            if (!esValido)
            {
                return ValidationProblem();
            }
            _mapper.Map(autorPatch, autorDb);
            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }


       [HttpDelete("{id:int}", Name = "BorrarAutorV1")]
       public async Task<ActionResult> Delete(int id)
        {

            var autor = await _context.Autores.FirstOrDefaultAsync(x => x.Id == id);

            if(autor is null)
            {
                return NotFound();
            }

            _context.Remove(autor);
            await _context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);

            await almacenadorArchivos.Borrar(autor.Foto, contenedor);

            return NoContent(); 

        }
    }
}
