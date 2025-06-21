using System.ComponentModel.DataAnnotations;

namespace RecomendadorDePeliculas.Entidades.Metadata
{
    public class UsuarioMetadataEdicion
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido")]
        public string Correo { get; set; } = null!;

        [DataType(DataType.Date, ErrorMessage = "Debe tener una fecha válida")]
        public DateOnly? FechaDeNacimiento { get; set; }

        [Display(Name = "Nueva contraseña")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Debe tener al menos 6 caracteres")]
        public string ContraseniaHash { get; set; } = null!;

        [Compare("ContraseniaHash", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasenia { get; set; } = null!;

        public string? Genero { get; set; }
    }
}
