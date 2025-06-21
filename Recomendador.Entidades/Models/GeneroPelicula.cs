using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecomendadorDePeliculas.Entidades.Models;

public partial class GeneroPelicula
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    public virtual ICollection<UsuarioGenero> UsuarioGeneros { get; set; } = new List<UsuarioGenero>();
}