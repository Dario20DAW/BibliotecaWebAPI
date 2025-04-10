using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{


    [ApiController]
    [Route("api/autores")]
    [Authorize(Policy = "esadmin")]

    public class AutoresController: ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private const string contenedor = "autores";


        public AutoresController(ApplicationDbContext contex, IMapper mapper,
            IAlmacenadorArchivos almacenadorArchivos)
        {
            _context = contex;
            _mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<AutorDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {


            var queryable =   _context.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var autores = await queryable
                                .OrderBy(x => x.Nombres)
                                .Paginar(paginacionDTO).ToListAsync();
            var autoresDTO = _mapper.Map<IEnumerable<AutorDTO>>(autores);
            return autoresDTO;
        }



        [HttpGet("{id:int}", Name = "ObtenerAutor")]
        [AllowAnonymous]
        [EndpointSummary("Obtiene autor por Id")]
        [EndpointDescription("Obtiene autor por id y sus libros, si no existe se devuelve 404")]
        public async Task<ActionResult<AutorConLibrosDTO>> Get(int id)
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





         [HttpPost]
         public async Task<ActionResult> Post(AutorCrearDTO autorCrearDTO)
         {          
            var autor = _mapper.Map<Autor>(autorCrearDTO);  
            _context.Add(autor);
            await _context.SaveChangesAsync();
            var autorDTO = _mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("ObtenerAutor", new {id = autor.Id}, autorDTO);
         }



        [HttpPost("con-foto")]
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
            var autorDTO = _mapper.Map<AutorDTO>(autor);

            return CreatedAtRoute("ObtenerAutor", new { id = autor.Id }, autorDTO);
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, AutorCrearDTO autorCrearDTO)
        {
            var autor = _mapper.Map<Autor>(autorCrearDTO);
            autor.Id = id;
            _context.Update(autor);
            await _context.SaveChangesAsync();
            return NoContent();
        }



        [HttpPatch("{id:int}")]
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

            return NoContent();
        }


       [HttpDelete("{id:int}")]
       public async Task<ActionResult> Delete(int id)
        {

           var registrosBorrados = await _context.Autores.Where(x => x.Id == id).ExecuteDeleteAsync();

            if(registrosBorrados == 0)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
