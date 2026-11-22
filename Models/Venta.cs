
using System;
using System.Collections.Generic;

namespace GymControl.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public DateTimeOffset FechaVenta { get; set; }

        // Total bruto antes de aplicar el descuento
        public decimal Total { get; set; }

        // Nombre del vendedor que registra la venta
        public string Vendedor { get; set; } = string.Empty;

        // Porcentaje de descuento permitido: 0% a 20%
        public decimal PorcentajeDescuento { get; set; } = 0;

        // Importe monetario descontado
        public decimal MontoDescuento { get; set; } = 0;

        // Total que finalmente paga el cliente
        public decimal TotalFinal { get; set; }

        public string Estado { get; set; } = "Confirmada";

        public List<DetalleVenta> Detalles { get; set; } = new();

        // Aplicar y validar el descuento
        public void AplicarDescuento(decimal porcentaje)
        {
            if (Total < 0)
            {
                throw new ArgumentException(
                    "El total de la venta no puede ser negativo.");
            }

            if (porcentaje < 0 || porcentaje > 20)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(porcentaje),
                    "El descuento debe estar entre 0% y 20%.");
            }

            PorcentajeDescuento = porcentaje;

            MontoDescuento = Math.Round(
                Total * porcentaje / 100m,
                2,
                MidpointRounding.AwayFromZero
            );

            TotalFinal = Total - MontoDescuento;
        }
    }
}
