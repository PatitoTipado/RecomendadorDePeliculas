using System;
using System.Collections.Generic;

namespace RecomendadorDePeliculas.Entidades.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Correo { get; set; } = null!;

    public DateOnly? FechaDeNacimiento { get; set; }

    public string ContraseniaHash { get; set; } = null!;

    public string? Genero { get; set; }
}
