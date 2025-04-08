using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers
{


    [ApiController]
    [Route("api/libros")]
    [Authorize]
    public class LibrosController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LibrosController(ApplicationDbContext context, IMapper mapper )
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IEnumerable<LibroDTO>> Get()
        {
            var libros = await _context.Libros.ToListAsync();
            var librosDTO = _mapper.Map<IEnumerable<LibroDTO>>(libros);

            return librosDTO;

        }


        [HttpGet("{id:int}", Name = "ObtenerLibro")]
        public async Task<ActionResult<LibroConAutoresDTO>> Get(int id)
        {
            var libro = await _context.Libros
                .Include(y => y.Autores)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libro == null)
            {
                return NotFound();
            }

            var libroDTO = _mapper.Map<LibroConAutoresDTO>(libro);

            return libroDTO;

        }

        [HttpPost]
        public async Task<ActionResult> Post(LibroCrearDTO libroCrearDTO)
        {
            //validar que se envien autores
            if(libroCrearDTO.AutoresIds is null || libroCrearDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCrearDTO.AutoresIds),
                    "No se puede crear un libro sin autores");

                return ValidationProblem();
            }


            var autoresIdsExisten = await _context.Autores.Where(x => libroCrearDTO
                .AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            //validar que existan los ids
            if (autoresIdsExisten.Count != libroCrearDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCrearDTO.AutoresIds.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCrearDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }

            var libro = _mapper.Map<Libro>(libroCrearDTO);
            AsignarOrdenAutores(libro);

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            var libroDTO = _mapper.Map<LibroDTO>(libro);


            return CreatedAtRoute("ObtenerLibro", new { id = libro.Id }, libroDTO);
        }


        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.Autores is not null)
            {
                for (int i = 0; i < libro.Autores.Count; i++)
                {
                    libro.Autores[i].Orden = i;
                }
            }
        }




        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, LibroCrearDTO libroCrearDTO)
        {
            //validar que se envien autores
            if (libroCrearDTO.AutoresIds is null || libroCrearDTO.AutoresIds.Count == 0)
            {
                ModelState.AddModelError(nameof(libroCrearDTO.AutoresIds),
                    "No se puede crear un libro sin autores");

                return ValidationProblem();
            }


            var autoresIdsExisten = await _context.Autores.Where(x => libroCrearDTO
                .AutoresIds.Contains(x.Id))
                .Select(x => x.Id).ToListAsync();

            //validar que existan los ids
            if (autoresIdsExisten.Count != libroCrearDTO.AutoresIds.Count)
            {
                var autoresNoExisten = libroCrearDTO.AutoresIds.Except(autoresIdsExisten);
                var autoresNoExistenString = string.Join(", ", autoresNoExisten);
                var mensajeError = $"Los siguientes autores no existen: {autoresNoExistenString}";
                ModelState.AddModelError(nameof(libroCrearDTO.AutoresIds), mensajeError);
                return ValidationProblem();
            }


            //aqui traemos los datos de los autores tambien
            var libroDb  = await _context.Libros
                .Include(x => x.Autores)
                .FirstOrDefaultAsync(x => x.Id == id);


            if (libroDb == null) { 
                
                return NotFound();
            }

            //con este mapeo se consigue que tambien se actualice la info de los autores
            //correspondiente al libro 
            libroDb = _mapper.Map(libroCrearDTO, libroDb);
            AsignarOrdenAutores(libroDb);


            await _context.SaveChangesAsync();
            return Ok();

        }



        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await _context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }
            return Ok();
        }

    }
}