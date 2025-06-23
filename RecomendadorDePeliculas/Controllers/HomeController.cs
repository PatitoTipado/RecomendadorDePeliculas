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
        private IPeliculasLogica peliculasLogica;
        private IGeneroLogica _generoLogica;

        public HomeController(IRecomenderLogica peliculaLogica, ITmdbLogica tmdbLogica, IPeliculasLogica peliculasLogica, IGeneroLogica generoLogica)
        {
            _peliculaLogica = peliculaLogica;
            _tmdbLogica = tmdbLogica;
            this.peliculasLogica = peliculasLogica;
            _generoLogica = generoLogica;
        }

        public IActionResult Generos()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var generos = _generoLogica.ObtenerTodosLosGeneros();

            var generosPreferidos = _generoLogica.ObtenerGenerosFavoritos(userId);

            ViewBag.Generos = generos;
            ViewBag.GenerosPreferidos = generosPreferidos;

            return View();
        }

        [HttpPost]
        public IActionResult GuardarPreferencias(List<int> generosSeleccionados)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            if (generosSeleccionados.Count < 2 || generosSeleccionados.Count > 3)
            {
                ModelState.AddModelError("", "Debes seleccionar entre 2 y 3 géneros.");
                ViewBag.Generos = _generoLogica.ObtenerTodosLosGeneros();
                return View("Generos");
            }

            _generoLogica.GuardarPreferencias(userId, generosSeleccionados);

            return RedirectToAction("CalificarPeliculas");
        }


        [HttpGet]
        public IActionResult CalificarPeliculas()
        {
            int userId = Int32.Parse(HttpContext.Session.GetString("UserId"));

            if (_generoLogica.noPoseeGeneros(userId))
            {
                return RedirectToAction("Generos");
            }

            var generosPreferidos = _generoLogica.ObtenerGenerosPreferidos(userId);
            var generosFinales = generosPreferidos;

            var favoritas = _generoLogica.ObtenerGenerosDePeliculasFavoritas(userId);

            if (favoritas.Count > 0)
            {

                var generosDesdeHistorial = ParsearPeliculas(favoritas);

                generosFinales = generosPreferidos
                  .Union(generosDesdeHistorial)
                  .ToArray();
            }

            TempData["generos"] = generosFinales;


            List<Pelicula> pelicula = _peliculaLogica.ObtenerPeliculasACalificarQueNoCalificoAntes(userId, generosFinales);

            if (pelicula.Count == 0)
            {
                TempData["Mensaje"] = "No hay películas disponibles con los géneros seleccionados seleccione otro genero.";
                return RedirectToAction("Generos");
            }

            _tmdbLogica.ConseguirPeliculas(pelicula.First().Id);
            List<PeliculaCalificacionDTO> peliculas = _tmdbLogica.obtenerCaracteristicasDePeliculas(pelicula);
            return View(pelicula);
        }

        private string[] ParsearPeliculas(List<string> favoritas)
        {
            return favoritas.SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .Select(g => g.Trim())
                .Distinct()
                .ToArray();
        }

        [HttpGet]
        public IActionResult obtener()
        {
            return View(_tmdbLogica.ConseguirPeliculas(376670));
        }

        [HttpGet]
        public void HacerPrediccion()
        {
            _peliculaLogica.RealizarPrediccion(1, 25);

            Console.WriteLine("la pelicula es recomendada ");
        }

        [HttpGet]
        public IActionResult Resenar([FromQuery] int id)
        {
            var pelicula = _peliculaLogica.ObtenerPeliculaPorId(id);
            if (pelicula == null) return NotFound();

            return View("Resenar", pelicula);
        }


        [HttpPost]
        public IActionResult Resenar(int peliculaId, double calificacion, string comentario, string peliculaGenero)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            _peliculaLogica.GuardarResena(userId, peliculaId, calificacion, comentario, peliculaGenero);

            return RedirectToAction("CalificarPeliculas");
        }

        [HttpPost]
        public IActionResult EliminarResena(int peliculaId)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            _peliculaLogica.EliminarResena(userId, peliculaId);

            return RedirectToAction("Historial");
        }

        [HttpGet]
        public IActionResult Historial()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var historialUsuario = _peliculaLogica.ObtenerHistorialDeUsuario(userId);

            return View(historialUsuario);
        }


        [HttpGet]
        public IActionResult BuscarPeliculas(string titulo)
        {
            var resultados = peliculasLogica.BuscarPeliculasPorTitulo(titulo);
            return View("CalificarPeliculas", resultados);
        }

    }
}
