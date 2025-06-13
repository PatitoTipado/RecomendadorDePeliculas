using System;
using System.Collections.Generic;

namespace RecomendadorDePeliculas.Entidades.Models;

public partial class Pelicula
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Genres { get; set; }

    public int? TmdbId { get; set; }
}
