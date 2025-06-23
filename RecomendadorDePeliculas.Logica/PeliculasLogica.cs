
using CsvHelper;
using CsvHelper.Configuration;
using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;
using System.Globalization;

namespace RecomendadorDePeliculas.Logica
{
    public interface IPeliculasLogica
    {
        public List<Pelicula> obtenerPeliculas(List<int> movieIdsAExcluir, params string[] generos);
        public List<Pelicula> obtenerPeliculas(params string[] generos);
        public Pelicula ObtenerPeliculaPorId(int id);
        List<Pelicula> BuscarPeliculasPorTitulo(string titulo);
        List<HistorialConInfoDTO> ObtenerHistorialConInfo(int usuarioId);

    }
    public class PeliculasLogica : IPeliculasLogica
    {
        private string _moviePath;
        private readonly RecomendadorPeliculasContext _context;
        private readonly ITmdbLogica _tmdbLogica;

        private Dictionary<string, string> traducciones = new Dictionary<string, string>{
            { "Acción", "Action" },
            { "Comedia", "Comedy" },
            { "Drama", "Drama" },
            { "Terror", "Horror" },
            { "Ciencia Ficción", "Sci-Fi" },
            { "Romance", "Romance" },
            { "Aventura", "Adventure" }
        };

        public PeliculasLogica(string moviePath, RecomendadorPeliculasContext context, ITmdbLogica tmdbLogica)
        {
            _moviePath = moviePath;
            _context = context;
            _tmdbLogica = tmdbLogica;
        }


        public List<Pelicula> obtenerPeliculas(List<int> movieIdsAExcluir, params string[] generos)
        {
            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var peliculas = csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim();
                    //Console.WriteLine($"TmdbId bruto: {tmdbIdRaw}");

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0,
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && tmdbIdRaw.Contains(".")
                        ? int.TryParse(tmdbIdRaw.Split('.')[0], out int cleanId) ? cleanId : 0
                        : int.TryParse(tmdbIdRaw, out int directId) ? directId : 0

                    };
                }).ToList();

                var generosTraducidos = generos
                    .Where(g => traducciones.ContainsKey(g))
                    .Select(g => traducciones[g])
                    .ToArray();

                var peliculasFiltradas = peliculas
                    .Where(p =>
                        generosTraducidos.Any(g => p.Genres.Split('|').Contains(g))
                    )
                    .Where(p => !movieIdsAExcluir.Contains(p.Id))
                    .OrderBy(x => Guid.NewGuid())
                    .Take(20)
                    .ToList();

                return peliculasFiltradas;
            }
        }

        public Pelicula ObtenerPeliculaPorId(int id)
        {
            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var peliculas = csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim();

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0,
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && float.TryParse(tmdbIdRaw, out float tmdbIdFloat)
                            ? (int)tmdbIdFloat
                            : 0
                    };
                }).ToList();

                return peliculas.FirstOrDefault(p => p.Id == id);
            }
        }


        public List<Pelicula> obtenerPeliculas(params string[] generos)
        {
            return obtenerPeliculas(new List<int>(), generos);
        }


        public List<Pelicula> BuscarPeliculasPorTitulo(string titulo)
        {
            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var peliculas = csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim();
                    //Console.WriteLine($"TmdbId bruto: {tmdbIdRaw}");

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0,
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && tmdbIdRaw.Contains(".")
                        ? int.TryParse(tmdbIdRaw.Split('.')[0], out int cleanId) ? cleanId : 0
                        : int.TryParse(tmdbIdRaw, out int directId) ? directId : 0

                    };
                }).ToList();

                if (!string.IsNullOrWhiteSpace(titulo))
                {
                    peliculas = peliculas
                        .Where(p => p.Title.Contains(titulo, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return peliculas;
            }
        }

        private List<Pelicula> CargarPeliculasDesdeCsv()
        {
            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                return csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim();

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0,
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && tmdbIdRaw.Contains(".")
                            ? int.TryParse(tmdbIdRaw.Split('.')[0], out int cleanId) ? cleanId : 0
                            : int.TryParse(tmdbIdRaw, out int directId) ? directId : 0
                    };
                }).ToList();
            }
        }


        public List<HistorialConInfoDTO> ObtenerHistorialConInfo(int usuarioId)
        {
            var historial = _context.Historials
                .Where(h => h.UsuarioId == usuarioId)
                .ToList();

            var peliculas = CargarPeliculasDesdeCsv(); // O método equivalente
            var historialConInfo = new List<HistorialConInfoDTO>();

            foreach (var reseña in historial)
            {
                var peli = peliculas.FirstOrDefault(p => p.Id == reseña.PeliculaId);
                if (peli != null)
                {
                    var detalles = peli.TmdbId.HasValue ? _tmdbLogica.ConseguirPeliculas(peli.TmdbId.Value, "es-ES") : null;

                    historialConInfo.Add(new HistorialConInfoDTO
                    {
                        PeliculaId = peli.Id,
                        Titulo = peli.Title,
                        Generos = peli.Genres,
                        ImagenUrl = detalles?.PosterPath != null
                            ? $"https://image.tmdb.org/t/p/w500{detalles.PosterPath}"
                            : "/img/no-disponible.jpg",
                        Calificacion = reseña.Calificacion,
                        Comentario = reseña.Comentario,
                        FechaResena = reseña.FechaReseña
                    });
                }
            }

            return historialConInfo;
        }



    }
}
