using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using RecomendadorDePeliculas.Logica;

namespace RecomendadorDePeliculas.Entidades.Models
{
    public class UsuarioEditarViewModel : IValidatableObject
    {
        public int Id { get; set; }

        [EmailAddress(ErrorMessage = "Debe ser un correo electrónico válido")]
        public string Correo { get; set; } = null!;

        [DataType(DataType.Date)]
        [Display(Name = "Fecha de nacimiento")]
        public DateOnly? FechaDeNacimiento { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        public string? Contrasenia { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string? ConfirmarContrasenia { get; set; }

        [Display(Name = "Género")]
        public string? Genero { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var usuarioLogica = validationContext.GetService<IUsuarioLogica>();

            if (usuarioLogica != null && usuarioLogica.CorreoEnUsoPorOtroUsuario(Id, Correo))
            {
                yield return new ValidationResult("Este correo ya está registrado por otro usuario", new[] { nameof(Correo) });
            }

            if (!string.IsNullOrWhiteSpace(Contrasenia))
            {
                if (Contrasenia != ConfirmarContrasenia)
                {
                    yield return new ValidationResult("Las contraseñas no coinciden", new[] { nameof(ConfirmarContrasenia) });
                }
            }
        }
    }
}
