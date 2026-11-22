
using Xunit;
using GymControl.Models;

namespace GymControl.Tests
{
    public class VentaTests
    {
        // PRUEBA 1: Descuento válido del 10%
        [Fact]
        public void AplicarDescuento_DiezPorCiento_CalculaCorrectamente()
        {
            var venta = new Venta
            {
                Total = 270m
            };

            venta.AplicarDescuento(10m);

            Assert.Equal(27m, venta.MontoDescuento);
            Assert.Equal(243m, venta.TotalFinal);
        }

        // PRUEBA 2: Límite exacto del 20%
        [Fact]
        public void AplicarDescuento_VeintePorCiento_PermiteDescuento()
        {
            var venta = new Venta
            {
                Total = 270m
            };

            venta.AplicarDescuento(20m);

            Assert.Equal(54m, venta.MontoDescuento);
            Assert.Equal(216m, venta.TotalFinal);
        }

        // PRUEBA 3: Descuento superior al 20%
        [Fact]
        public void AplicarDescuento_MayorVeintePorCiento_LanzaExcepcion()
        {
            var venta = new Venta
            {
                Total = 270m
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => venta.AplicarDescuento(20.01m)
            );
        }

        // PRUEBA 4: Descuento negativo
        [Fact]
        public void AplicarDescuento_Negativo_LanzaExcepcion()
        {
            var venta = new Venta
            {
                Total = 270m
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => venta.AplicarDescuento(-5m)
            );
        }
    }
}
