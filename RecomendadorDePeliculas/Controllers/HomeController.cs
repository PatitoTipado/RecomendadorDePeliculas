using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.DTOS;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using TMDbLib.Objects.Movies;

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

            var peliculas = _peliculaLogica.ObtenerPeliculasACalificarQueNoCalificoAntes(userId, generosFinales);

            var peliculasConImagen = new List<PeliculaConImagenDTO>();

            foreach (var pelicula in peliculas)
            {
                if (pelicula.Adult == false)
                {
                    string? imagen = null;
                    Movie? detalles = null;

                    if (pelicula.TmdbId.HasValue && pelicula.TmdbId > 0)
                    {
                        detalles = _tmdbLogica.ConseguirPeliculas(pelicula.TmdbId.Value);
                        imagen = detalles?.PosterPath != null
                            ? $"https://image.tmdb.org/t/p/w500{detalles.PosterPath}"
                            : null;
                    }

                    peliculasConImagen.Add(new PeliculaConImagenDTO
                    {
                        Id = pelicula.Id,
                        Title = pelicula.Title,
                        Genres = pelicula.Genres,
                        TmdbId = pelicula.TmdbId,
                        ImagenUrl = imagen,
                        Sinopsis = detalles?.Overview
                    });
                }
            }


            return View(peliculasConImagen);
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
            var historial = peliculasLogica.ObtenerHistorialConInfo(userId);
            return View(historial);
        }



        [HttpGet]
        public IActionResult BuscarPeliculas(string titulo)
        {
            var peliculas = peliculasLogica.BuscarPeliculasPorTitulo(titulo);

            var peliculasConImagen = new List<PeliculaConImagenDTO>();

            foreach (var pelicula in peliculas)
            {
                string? imagen = null;
                Movie? detalles = null;

                if (pelicula.TmdbId.HasValue && pelicula.TmdbId > 0)
                {
                    detalles = _tmdbLogica.ConseguirPeliculas(pelicula.TmdbId.Value);
                    imagen = detalles?.PosterPath != null
                        ? $"https://image.tmdb.org/t/p/w500{detalles.PosterPath}"
                        : null;
                }

                peliculasConImagen.Add(new PeliculaConImagenDTO
                {
                    Id = pelicula.Id,
                    Title = pelicula.Title,
                    Genres = pelicula.Genres,
                    TmdbId = pelicula.TmdbId,
                    ImagenUrl = imagen,
                    Sinopsis = detalles?.Overview
                });
            }

            return View("CalificarPeliculas", peliculasConImagen);
        }


    }
}
