
namespace GymControl.Models
{
    public class Venta
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }

        public string NombreCliente { get; set; } = string.Empty;

        public DateTimeOffset FechaVenta { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = "Confirmada";

        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
