using System.ComponentModel.DataAnnotations;
using RecomendadorDePeliculas.Entidades.Metadata;

namespace RecomendadorDePeliculas.Entidades.Models
{
    [MetadataType(typeof(UsuarioMetadata))]
    public partial class Usuario
    {
    }
}
