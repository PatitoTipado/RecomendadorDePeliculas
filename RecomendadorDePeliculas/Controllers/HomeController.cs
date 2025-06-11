using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Logica;
using RecomendadorDePeliculas.Entidades.DTOS;

namespace RecomendadorDePeliculas.Web.Controllers
{
    public class HomeController : Controller
    {
        private IPeliculaLogica _peliculaLogica;
        
        public HomeController(IPeliculaLogica peliculaLogica)
        {
            _peliculaLogica = peliculaLogica;
        }

        public IActionResult Index()
        {
            //mostrar recomendaciones
            return View();
        }

        //pasarela para puntear 10 pelis
        [HttpPost]
        public IActionResult EvaluarPeliculas()
        {
            UsuarioPeliculaCalificacionDTO dto = _peliculaLogica.ObtenerPeliculasACalificar();

            return View();
        }

        [HttpGet]
        public void HacerPrediccion()
        {
            _peliculaLogica.RealizarPrediccion(1,25);

            Console.WriteLine("la pelicula es recomendada ");
        }
    }
}
