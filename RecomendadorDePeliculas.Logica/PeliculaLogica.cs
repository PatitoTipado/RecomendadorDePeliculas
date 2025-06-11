using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliulas.ML;

namespace RecomendadorDePeliculas.Logica
{
    public interface IPeliculaLogica
    {
        UsuarioPeliculaCalificacionDTO ObtenerPeliculasACalificar();
        void RealizarPrediccion(int v, int v1);
    }

    public class PeliculaLogica : IPeliculaLogica
    {
        IModelMovieRecomender _modelRecomender;
        public PeliculaLogica(IModelMovieRecomender model)
        {
            _modelRecomender=model;
        }
        public UsuarioPeliculaCalificacionDTO ObtenerPeliculasACalificar()
        {

            return null;
        }

        public void RealizarPrediccion(int v, int v1)
        {
            _modelRecomender.UseModelForSinglePrediction(v, v1);
        }
    }
}
