
using System;
using System.ComponentModel.DataAnnotations;

namespace GymControl.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar una membresía")]
        public int MembresiaId { get; set; }

        [Range(typeof(decimal), "0.01", "99999999.99",
            ErrorMessage = "El monto debe ser mayor que cero")]
        public decimal Monto { get; set; }

        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Seleccione un método de pago")]
        public string MetodoPago { get; set; } = "";

        public string NombreCliente { get; set; } = "";

        public string TipoMembresia { get; set; } = "";
    }
}
