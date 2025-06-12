
using CsvHelper;
using CsvHelper.Configuration;
using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;
using System.Globalization;

namespace RecomendadorDePeliculas.Logica
{
    public interface IPeliculasLogica
    {
        public List<Pelicula> obtenerPeliculas(List<int> movieIdsAExcluir, string generoDePreferencia, string segundoGenero);
        List<Pelicula> obtenerPeliculas(string preferencia, string preferenciaSecundaria);
    }
    public class PeliculasLogica : IPeliculasLogica
    {
        private string _moviePath;

        public PeliculasLogica(string moviePath)
        {
            _moviePath = moviePath;
        }

        public List<Pelicula> obtenerPeliculas(List<int> movieIdsAExcluir, string generoDePreferencia, string segundoGenero)
        {

            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                var peliculas = csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim(); // Asegurar que sea string y limpiar espacios

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0, // Si falla, asigna 0
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && float.TryParse(tmdbIdRaw, out float tmdbIdFloat)
                            ? (int)tmdbIdFloat // Convertir correctamente
                            : 0 // Si está vacío, asignar 0
                    };
                }).ToList();

                var peliculasFiltradas = peliculas
                    .Where(p => p.Genres.Split('|').Contains(generoDePreferencia) || p.Genres.Split('|').Contains(segundoGenero)) // Filtrar por géneros
                    .Where(p => !movieIdsAExcluir.Contains(p.Id)) // Excluir películas por ID
                    .OrderBy(x => Guid.NewGuid()) // Ordenar aleatoriamente
                    .Take(20) // Obtener 10 películas aleatorias
                    .ToList();

                return peliculasFiltradas;
            }
        }

        public List<Pelicula> obtenerPeliculas(string preferencia, string preferenciaSecundaria)
        {
            using (var reader = new StreamReader(_moviePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {

                var peliculas = csv.GetRecords<dynamic>().Select(p =>
                {
                    string tmdbIdRaw = p.tmdbId?.ToString().Trim(); // Asegurar que sea string y limpiar espacios

                    return new Pelicula
                    {
                        Id = int.TryParse(p.movieId?.ToString(), out int movieId) ? movieId : 0, // Si falla, asigna 0
                        Title = p.title,
                        Genres = p.genres,
                        TmdbId = !string.IsNullOrEmpty(tmdbIdRaw) && float.TryParse(tmdbIdRaw, out float tmdbIdFloat)
                            ? (int)tmdbIdFloat // Convertir correctamente
                            : 0 // Si está vacío, asignar 0
                    };
                }).ToList();



                var peliculasFiltradas = peliculas
                    .Where(p => p.Genres.Split('|').Contains(preferencia) || p.Genres.Split('|').Contains(preferenciaSecundaria)) // Filtrar por géneros
                    .OrderBy(x => Guid.NewGuid()) // Ordenar aleatoriamente
                    .Take(20) // Obtener 10 películas aleatorias
                    .ToList();

                return peliculasFiltradas;

            }
        }
    }
}
