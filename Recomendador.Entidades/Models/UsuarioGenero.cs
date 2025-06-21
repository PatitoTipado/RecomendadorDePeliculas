
namespace RecomendadorDePeliculas.Entidades.Models;

public partial class UsuarioGenero
{
    public int UsuarioId { get; set; }
    public int GeneroId { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
    public virtual GeneroPelicula Genero { get; set; } = null!;
}