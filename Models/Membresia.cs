
using System;
using System.ComponentModel.DataAnnotations;

namespace GymControl.Models
{
    public class Membresia
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "El tipo de membresía es obligatorio")]
        public string Tipo { get; set; } = "";

        [Range(typeof(decimal), "0", "99999999",
            ErrorMessage = "El precio debe ser válido")]
        public decimal Precio { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; } = DateTime.Today.AddMonths(1);

        [Required(ErrorMessage = "El estado es obligatorio")]
        public string Estado { get; set; } = "Activa";

        // Propiedad auxiliar para mostrar el cliente
        public string NombreCliente { get; set; } = "";
    }
}
