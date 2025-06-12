using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Logica;
using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;

namespace RecomendadorDePeliculas.Web.Controllers
{
    public class HomeController : Controller
    {
        private IRecomenderLogica _peliculaLogica;
        private ITmdbLogica _tmdbLogica;

        public HomeController(IRecomenderLogica peliculaLogica,ITmdbLogica tmdbLogica)
        {
            _peliculaLogica = peliculaLogica;
            _tmdbLogica= tmdbLogica;
        }

        public IActionResult Index()
        {
            //mostrar recomendaciones
            return View();
        }

        //pasarela para puntear 10 pelis
        [HttpGet]
        public IActionResult CalificarPeliculas()
        {
            //listar generos
            int userId = Int32.Parse(HttpContext.Session.GetString("UserId"));
            List<Pelicula> pelicula = _peliculaLogica.ObtenerPeliculasACalificarQueNoCalificoAntes(userId, "Romance", "Comedy");
            _tmdbLogica.ConseguirPeliculas(pelicula.First().Id);
            List < PeliculaCalificacionDTO> peliculas = _tmdbLogica.obtenerCaracteristicasDePeliculas(pelicula);
            return View(pelicula);
        }

        [HttpGet]
        public IActionResult obtener()
        {
            return View(_tmdbLogica.ConseguirPeliculas(376670));
        }

        [HttpGet]
        public void HacerPrediccion()
        {
            _peliculaLogica.RealizarPrediccion(1,25);

            Console.WriteLine("la pelicula es recomendada ");
        }

    }
}
