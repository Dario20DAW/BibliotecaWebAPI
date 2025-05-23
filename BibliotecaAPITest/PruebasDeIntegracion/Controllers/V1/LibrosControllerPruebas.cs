﻿using BibliotecaAPI.DTOs;
using BibliotecaAPITest.Utilidades;
using BibliotecaAPITests;
using System.Net;

namespace BibliotecaAPITests.PruebasDeIntegracion.Controllers.V1
{
    [TestClass]
    public class LibrosControllerPruebas : BasePruebas
    {
        private readonly string url = "/api/v1/libros";
        private string nombreBD = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Post_Devuelve400_CuandoAutoresIdsEsVacio()
        {
            // Preparación
            var factory = ConstruirWebApplicationFactory(nombreBD);
            var cliente = factory.CreateClient();
            var libroCreacionDTO = new LibroCrearDTO { Titulo = "Título" };

            // Prueba
            var respuesta = await cliente.PostAsJsonAsync(url, libroCreacionDTO);

            // Verificación
            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: respuesta.StatusCode);
        }
    }
}