using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecomendadorDePeliculas.Entidades.DTOS
{
    public class PeliculaConImagenDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Genres { get; set; }
        public int? TmdbId { get; set; }

        public string? ImagenUrl { get; set; } 

    }

}
