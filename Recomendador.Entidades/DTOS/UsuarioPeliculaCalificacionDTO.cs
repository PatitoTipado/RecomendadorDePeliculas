namespace RecomendadorDePeliculas.Entidades.DTOS
{
    public class UsuarioPeliculaCalificacionDTO
    {
        public string Correo { get; set; }  // Usado para obtener el UserId de forma interna
        public int UserId { get; set; }     // Se rellena después de obtenerlo por el correo

        public List<PeliculaCalificacionDTO> MovieRatings { get; set; } = new();
    }
}
