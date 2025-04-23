using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V2
{

    [ApiController]
    [Route("api/v2/libros/{libroId:int}/comentarios")]
    [Authorize(Policy = "esadmin")]
    public class ComentariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IOutputCacheStore _cacheStore;
        private const string cache = "comentarios-obtener";

        public ComentariosController(ApplicationDbContext context, IMapper mapper,
            IServicioUsuarios servicioUsuarios, IOutputCacheStore cacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioUsuarios = servicioUsuarios;
            _cacheStore = cacheStore;
        }

        [HttpGet]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List<ComentarioDTO>>> Get(int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarios = await context.Comentarios
                .Include(x => x.Usuario)
                .Where(x => x.libroId == libroId)
                .OrderByDescending(x => x.FechaPublicacion)
                .ToListAsync();

            return mapper.Map<List<ComentarioDTO>>(comentarios);

        }


        [HttpGet("{id}", Name = "ObtenerComentarioV2")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<ComentarioDTO>> Get(Guid id)
        {
            var comentario = await context.Comentarios
                .Include (x => x.Usuario)
                .FirstOrDefaultAsync(x => x.id == id);
            if (comentario == null)
            {
                return NotFound();
            }

            return mapper.Map<ComentarioDTO>(comentario);
        }



        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCrearDTO comentarioCrearDTO)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario == null)
            {
                return NotFound();
            }

            var comentario = mapper.Map<Comentario>(comentarioCrearDTO);
            comentario.libroId = libroId;
            comentario.FechaPublicacion = DateTime.UtcNow;
            comentario.UsuarioId = usuario.Id;

            context.Add(comentario);
            await  context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);


            var comentarioDTO = mapper.Map<ComentarioDTO>(comentario);
            return CreatedAtRoute("ObtenerComentarioV2", new { id = comentario.id, libroId }, comentarioDTO);

        }


        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int libroId,  JsonPatchDocument<ComentarioPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario == null)
            {
                return NotFound();
            }

            var comentarioDb = await context.Comentarios.FirstOrDefaultAsync(x => x.id == id);

            if (comentarioDb is null)
            {
                return NotFound();
            }

            if (comentarioDb.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            var comentarioPatchDTO = mapper.Map<ComentarioPatchDTO>(comentarioDb);

            patchDoc.ApplyTo(comentarioPatchDTO, ModelState);

            var esValido = TryValidateModel(comentarioPatchDTO);

            if (!esValido)
            {
                return ValidationProblem();
            }

            mapper.Map(comentarioPatchDTO, comentarioDb);
            await context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);


            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, int libroId)
        {
            var existeLibro = await context.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var usuario = await servicioUsuarios.ObtenerUsuario();

            if (usuario == null)
            {
                return NotFound();
            }


            var comentarioDB = await context.Comentarios
                .FirstOrDefaultAsync(x => x.id == id);
            
            if (comentarioDB == null)
            {
                return NotFound();
            }

            if (comentarioDB.UsuarioId != usuario.Id)
            {
                return Forbid();
            }

            comentarioDB.EstaBorrado = true;
            context.Update(comentarioDB);
            await context.SaveChangesAsync();
            await _cacheStore.EvictByTagAsync(cache, default);


            return NoContent();
        }
    }
}
