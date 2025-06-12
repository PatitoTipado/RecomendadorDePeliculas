using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;
using TMDbLib.Client;
using TMDbLib.Objects.Movies;

namespace RecomendadorDePeliculas.Logica
{
    public interface ITmdbLogica
    {
        public Movie ConseguirPeliculas(int IdTmdb);
        List<PeliculaCalificacionDTO> obtenerCaracteristicasDePeliculas(List<Pelicula> peliculas);
    }
    public class TmdbLogica:ITmdbLogica
    {
        private string _apikey;
        private string _accesToken;
        private TMDbClient _client;


        public TmdbLogica(string apiKey,string accesToken)
        {
            _apikey=apiKey;
            _accesToken=accesToken;
            _client = new TMDbClient(_apikey);

        }

        public Movie ConseguirPeliculas(int idTmdb) {
            Movie movie = _client.GetMovieAsync(idTmdb).Result;
            return movie;
        }

        public List<PeliculaCalificacionDTO> obtenerCaracteristicasDePeliculas(List<Pelicula> peliculas)
        {



            return null;
        }
    }
}
