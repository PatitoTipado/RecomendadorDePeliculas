namespace RecomendadorDePeliculas.Entidades.DTOS
{
    public class PeliculaCalificacionDTO
    {
        public bool Calificada { get; set; }        // Si ya fue calificada por el usuario
        public string Titulo { get; set; }          // Título de la película
        public int MovieId { get; set; }            // ID interna de tu sistema
        public int Calificacion { get; set; }             // Valor de la calificación (1 a 10, por ej.)
        public int AnioLanzamiento { get; set; }               // Año de lanzamiento
        public string Generos { get; set; }         // Géneros como string, ej: "Action, Drama"
        public int TmdbId { get; set; }             // ID de la película en TheMovieDB
        public string ImagenUrl { get; set; }       // URL del póster desde TMDB
    }
}
