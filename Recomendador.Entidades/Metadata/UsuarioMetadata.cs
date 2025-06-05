using System.ComponentModel.DataAnnotations;

namespace RecomendadorDePeliculas.Entidades.Metadata;

public partial class UsuarioMetadata
{

        [Required]
        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido")]
        public string Correo { get; set; } = null!;

        [Required]
        [DataType(DataType.Date, ErrorMessage ="Debe tener una fecha de nacimiento valida")]
        public DateOnly? FechaDeNacimiento { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [Display(Name = "Asignar contraseña")]
        public string ContraseniaHash { get; set; } = null!;

        [Required(ErrorMessage ="El genero es obligatorio")]
        public string Genero { get; set; } =null!;

}
