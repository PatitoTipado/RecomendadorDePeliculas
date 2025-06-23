using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecomendadorDePeliculas.Entidades.DTOS
{
    public class HistorialConInfoDTO
    {
        public int PeliculaId { get; set; }
        public string Titulo { get; set; }
        public string ImagenUrl { get; set; }
        public string? Generos { get; set; }
        public double Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaResena { get; set; }
    }
}
