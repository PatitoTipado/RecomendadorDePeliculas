using System;
using System.Collections.Generic;

namespace RecomendadorDePeliculas.Entidades.Models;

public partial class Historial
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int PeliculaId { get; set; }

    public double Calificacion { get; set; }

    public DateTime FechaReseña { get; set; }

    public string? Comentario { get; set; }

    public bool IsCalificada { get; set; }
}
