
using System;
using System.ComponentModel.DataAnnotations;

namespace GymControl.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombres { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellidos { get; set; } = "";

        [Required(ErrorMessage = "El CI es obligatorio")]
        public string CI { get; set; } = "";

        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string? Correo { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Today;
    }
}
