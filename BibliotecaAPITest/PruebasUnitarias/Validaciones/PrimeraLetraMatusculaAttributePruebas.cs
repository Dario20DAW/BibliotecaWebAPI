using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BibliotecaAPI.Validaciones;

namespace BibliotecaAPITest.PruebasUnitarias.Validaciones
{

    [TestClass]
    public class PrimeraLetraMatusculaAttributePruebas
    {
        [TestMethod]
        [DataRow("")]
        [DataRow("  ")]
        [DataRow(null)]
        public void IsValid_RetornaExistoso_SiValueEsVacio(string value)
        {
            //preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);



            //verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);

        }


        [TestMethod]
        [DataRow("Felipe")]
        public void IsValid_RetornaExistoso_SiLaPrimeraLetraEsMayuscula(string value)
        {
            //preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);



            //verificacion
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado);

        }

        [TestMethod]
        [DataRow("felipe")]
        public void IsValid_RetornaError_SiValueTieneLaPrimeraLetraEsMinuscula(string value)
        {
            //preparacion
            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationContext = new ValidationContext(new object());

            //prueba
            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationContext);



            //verificacion
            Assert.AreEqual(expected: "La primera letra debe ser mayuscula", actual: resultado!.ErrorMessage);

        }
    }
}
