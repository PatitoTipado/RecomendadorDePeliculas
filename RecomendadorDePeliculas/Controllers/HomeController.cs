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
        private readonly IRecomendadorPeliculasContext _context;
        private IPeliculasLogica peliculasLogica;

        public HomeController(IRecomenderLogica peliculaLogica,ITmdbLogica tmdbLogica, IRecomendadorPeliculasContext context, IPeliculasLogica peliculasLogica)
        {
            _peliculaLogica = peliculaLogica;
            _tmdbLogica = tmdbLogica;
            _context = context;
            this.peliculasLogica = peliculasLogica;
        }

        public IActionResult Generos()
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var generos = _context.GenerosPeliculas.ToList();

            var generosPreferidos = _context.UsuarioGeneros
                .Where(ug => ug.UsuarioId == userId)
                .Select(ug => ug.GeneroId)
                .ToList();

            ViewBag.Generos = generos;
            ViewBag.GenerosPreferidos = generosPreferidos;

            return View();
        }

        [HttpPost]
        public IActionResult GuardarPreferencias(List<int> generosSeleccionados)
        {
            if (generosSeleccionados.Count < 2 || generosSeleccionados.Count > 3)
            {
                ModelState.AddModelError("", "Debes seleccionar entre 2 y 3 géneros.");
                ViewBag.Generos = _context.GenerosPeliculas.ToList();
                return View("Index");
            }

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var anteriores = _context.UsuarioGeneros.Where(x => x.UsuarioId == userId);
            _context.UsuarioGeneros.RemoveRange(anteriores);

            foreach (var idGenero in generosSeleccionados)
            {
                _context.UsuarioGeneros.Add(new UsuarioGenero
                {
                    UsuarioId = userId,
                    GeneroId = idGenero
                });
            }

            _context.SaveChanges();

            return RedirectToAction("CalificarPeliculas");
        }


        //pasarela para puntear 10 pelis
        [HttpGet]
        [HttpGet]
        public IActionResult CalificarPeliculas()
        {
            int userId = Int32.Parse(HttpContext.Session.GetString("UserId"));

            var generosPreferidos = _context.UsuarioGeneros
                                    .Where(ug => ug.UsuarioId == userId)
                                    .Select(ug => ug.Genero.Nombre)
                                    .ToArray();

            var favoritas = _context.Historials
                .Where(h => h.UsuarioId == userId && h.Calificacion > 3)
                .Select(h => h.Generos)
                .ToList();

            var generosFinales = favoritas.Count > 0
                ? generosPreferidos.Union(favoritas
                    .SelectMany(g => g.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(g => g.Trim())
                    .Distinct()).ToArray()
                : generosPreferidos;

            if (generosFinales.Length == 0)
            {
                TempData["Mensaje"] = "Primero seleccioná tus géneros favoritos.";
                return RedirectToAction("Index");
            }

            var peliculas = _peliculaLogica.ObtenerPeliculasACalificarQueNoCalificoAntes(userId, generosFinales);

            var peliculasConImagen = new List<PeliculaConImagenDTO>();

            foreach (var pelicula in peliculas)
            {
                if(pelicula.Adult==false)
                {                
                    string? imagen = null;

                    if (pelicula.TmdbId.HasValue && pelicula.TmdbId > 0)
                    {
                        var detalles = _tmdbLogica.ConseguirPeliculas(pelicula.TmdbId.Value);
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
                        ImagenUrl = imagen
                    });
                }
            }

            return View(peliculasConImagen);
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

        [HttpGet]
        public IActionResult Resenar([FromQuery] int id)
        {
            var pelicula = _peliculaLogica.ObtenerPeliculaPorId(id);
            if (pelicula == null) return NotFound();

            return View("Resenar", pelicula);
        }


        [HttpPost]
        public IActionResult Resenar(int peliculaId, double calificacion, string comentario,string peliculaGenero)
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
            var peliculas = peliculasLogica.BuscarPeliculasPorTitulo(titulo);

            var peliculasConImagen = new List<PeliculaConImagenDTO>();

            foreach (var pelicula in peliculas)
            {
                string? imagen = null;

                if (pelicula.TmdbId.HasValue && pelicula.TmdbId > 0)
                {
                    var detalles = _tmdbLogica.ConseguirPeliculas(pelicula.TmdbId.Value);
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
                    ImagenUrl = imagen
                });
            }

            return View("CalificarPeliculas", peliculasConImagen);
        }




    }
}
