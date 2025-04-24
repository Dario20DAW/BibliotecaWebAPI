using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Controllers.V1;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.Entidades;
using BibliotecaAPI.Servicios;
using BibliotecaAPI.Servicios.V1;
using BibliotecaAPITest.PruebasUnitarias.Utilidades;
using BibliotecaAPITests.Utilidades.Dobles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace BibliotecaAPITest.PruebasUnitarias.Controllers.V1
{

    [TestClass]
    public class AutoresControllerPruebas: BasePruebas
    {
        [TestMethod]
        public async Task Get_Retorna404_CuandoAutorConIdNoExiste()
        {


            // Preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();


            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores  servicioAutores = null!;


            var controller = new AutoresController(context, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Result as StatusCodeResult;
            Assert.AreEqual(expected: 404, actual: resultado!.StatusCode);

        }


        [TestMethod]
        public async Task Get_RetornaAutor_CuandoAutorConIdExiste()
        {


            // Preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();


            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;


            context.Autores.Add(new Autor { Nombres = "Darío", Apellidos = "Collar" });
            context.Autores.Add(new Autor { Nombres = "Inés", Apellidos = "Collar" });


            await context.SaveChangesAsync();

            var context2 = ConstruirContext(nombreBD);


            var controller = new AutoresController(context2, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);

        }

        [TestMethod]
        public async Task Get_RetornaAutorConLibros_CuandoAutorTieneLibros()
        {


            // Preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();


            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = null!;


            var libro1 = new Libro { Titulo = "Libro 1" };
            var libro2 = new Libro { Titulo = "Libro 2" };

            var autor = new Autor()
            {
                Nombres = "Felipe",
                Apellidos = "Gavilán",
                Libros = new List<AutorLibro>
                {
                    new AutorLibro{Libro = libro1},
                    new AutorLibro{Libro = libro2}
                }
            };
            context.Add(autor);

            await context.SaveChangesAsync();

            var context2 = ConstruirContext(nombreBD);


            var controller = new AutoresController(context2, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);

            // Prueba
            var respuesta = await controller.Get(1);

            // Verificacion
            var resultado = respuesta.Value;
            Assert.AreEqual(expected: 1, actual: resultado!.Id);
            Assert.AreEqual(expected: 2, actual: resultado.Libros.Count);


        }


        [TestMethod]
        public async Task Get_DebeLlamarGetDelServicioAutores()
        {
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();


            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = null!;
            IServicioAutores servicioAutores = Substitute.For<IServicioAutores>();


            var controller = new AutoresController(context, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);

            var paginacionDTO = new PaginacionDTO(2, 3);

            // Prueba
            await controller.Get(paginacionDTO);


            // Verificacion
            await servicioAutores.Received(1).Get(paginacionDTO);

        }


        [TestMethod]
        public async Task Post_DebeCrearAutor_CuandoEnviamosAutor()
        {

            // Preparacion
            var nombreBD = Guid.NewGuid().ToString();
            var context = ConstruirContext(nombreBD);
            var mapper = ConfigurarAutoMapper();


            IAlmacenadorArchivos almacenadorArchivos = null!;
            ILogger<AutoresController> logger = null!;
            IOutputCacheStore outputCacheStore = new OutputCacheStoreFalso();
            IServicioAutores servicioAutores = null!;

            var nuevoAutor = new AutorCrearDTO { Nombres = "nuevo", Apellidos = "nuevos" };

            var controller = new AutoresController(context, mapper, almacenadorArchivos,
                logger, outputCacheStore, servicioAutores);

            // Prueba
            var respuesta = await controller.Post(nuevoAutor);


            // Verificacion
            var resultado = respuesta as CreatedAtRouteResult;
            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContext(nombreBD);
            var cantidad = await contexto2.Autores.CountAsync();
            Assert.AreEqual(expected: 1, actual: cantidad);
        }
    }
}
