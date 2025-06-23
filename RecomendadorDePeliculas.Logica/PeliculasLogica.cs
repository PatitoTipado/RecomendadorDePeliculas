
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
    }
    public class PeliculasLogica : IPeliculasLogica
    {
        private string _moviePath;
        private Dictionary<string, string> traducciones = new Dictionary<string, string>{
            { "Acción", "Action" },
            { "Comedia", "Comedy" },
            { "Drama", "Drama" },
            { "Terror", "Horror" },
            { "Ciencia Ficción", "Sci-Fi" },
            { "Romance", "Romance" },
            { "Aventura", "Adventure" }
        };

        public PeliculasLogica(string moviePath)
        {
            _moviePath = moviePath;
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


    }
}
