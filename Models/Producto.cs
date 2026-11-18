
namespace GymControl.Models
{
    public class Producto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int DuracionDias { get; set; }

        public bool Activo { get; set; }

        public DateTimeOffset FechaCreacion { get; set; }
    }
}
