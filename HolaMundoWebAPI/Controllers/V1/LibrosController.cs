using AutoMapper;
using BibliotecaAPI.Datos;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaAPI.Controllers.V1
{


    [ApiController]
    [Route("api/v1/libros")]
    [Authorize(Policy = "esadmin")]

    public class LibrosController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IOutputCacheStore _cacheStore;
        private readonly ITimeLimitedDataProtector protectorLimitado;
        private const string cache = "libros-obtener";



        public LibrosController(ApplicationDbContext context, IMapper mapper,
            IDataProtectionProvider protectionProvider, IOutputCacheStore cacheStore)
        {
            _context = context;
            _mapper = mapper;
            _cacheStore = cacheStore;
            protectorLimitado = protectionProvider.CreateProtector("LibrosController")
                .ToTimeLimitedDataProtector();
        }


        [HttpGet("listado/obtener-token", Name ="ObtenerTokenV1")]
        public ActionResult ObtenerTokenListado()
        {
            var textoPlan = Guid.NewGuid().ToString();
            var token = protectorLimitado.Protect(textoPlan, lifetime: 
                TimeSpan.FromSeconds(30));

            var url = Url.RouteUrl("ObtenerListadoLibrosUsandoTokenV1", new { token }, "https");

            return Ok(new { url });
        }


        [HttpGet("listado/{token}", Name = "ObtenerListadoLibrosUsandoTokenV1")]
        [AllowAnonymous]

        public async Task<ActionResult> ObtenerListadoUsandoToken(string token)
        {
            try
            {
                protectorLimitado.Unprotect(token);
            }
            catch
            {
                ModelState.AddModelError(nameof(token), "El token ha expirado");
                return ValidationProblem();
            }


            var libros = await _context.Libros.ToListAsync();
            var librosDTO = _mapper.Map<IEnumerable<LibroDTO>>(libros);

            return Ok(librosDTO);

        }



        [HttpGet(Name = "ObtenerLibrosV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<IEnumerable<LibroDTO>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            
            

            var queryable = _context.Libros.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);
            var libros = await queryable
                                .OrderBy(x => x.Titulo)
                                .Paginar(paginacionDTO).ToListAsync();
            var librosDTO = _mapper.Map<IEnumerable<LibroDTO>>(libros);
            return librosDTO;

        }


        [HttpGet("{id:int}", Name = "ObtenerLibroV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
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

        [HttpPost(Name = "CrearLibroV1")]
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
            await _cacheStore.EvictByTagAsync(cache, default);


            var libroDTO = _mapper.Map<LibroDTO>(libro);


            return CreatedAtRoute("ObtenerLibroV1", new { id = libro.Id }, libroDTO);
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




        [HttpPut("{id:int}", Name = "ActualizarLibroV1")]
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
            await _cacheStore.EvictByTagAsync(cache, default);

            return Ok();

        }



        [HttpDelete("{id:int}", Name = "BorrarLibroV1")]
        public async Task<ActionResult> Delete(int id)
        {
            var registrosBorrados = await _context.Libros.Where(x => x.Id == id).ExecuteDeleteAsync();

            if (registrosBorrados == 0)
            {
                return NotFound();
            }

            await _cacheStore.EvictByTagAsync(cache, default);
            return Ok();
        }

    }
}