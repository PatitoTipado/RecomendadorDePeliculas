using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using static System.Formats.Asn1.AsnWriter;

namespace RecomendadorDePeliculas.Web.Controllers
{

    public class RecomendacionController : Controller
    {
        private IRecomenderLogica _peliculaLogica;
        private ITmdbLogica _tmdbLogica;
        private readonly IRecomendadorPeliculasContext _context;

        public RecomendacionController(IRecomenderLogica peliculaLogica, ITmdbLogica tmdbLogica, IRecomendadorPeliculasContext context)
        {
            _peliculaLogica = peliculaLogica;
            _tmdbLogica = tmdbLogica;
            _context = context;
        }
        [HttpPost]
        public IActionResult Recomendar([FromBody] RecomendacionRequest req)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            float score = _peliculaLogica.RealizarPrediccionScore(userId, req.peliculaId);
            
            //var ajuste = ObtenerAjustePorGeneros(req.generos, userId);
            // Sumar el ajuste al score base
            float scoreFinal = score;
            var mensaje = InterpretarResultado(score);
            var titulo = req.titulo;
            return Json(new { titulo, mensaje, score, scoreFinal });
        }

        private string InterpretarResultado(float score)
        {
            if (score >= 4) return "🟢 ¡Muy recomendable!";
            if (score >= 3) return "🟡 Puede gustarte";
            return "🔴 Poco probable que te guste";
        }

        private float ObtenerAjustePorGeneros(string generos, int usuario)
        {
            float ajuste = 0f;
            // Obtener géneros desde el request
            var generoPeliculaElegida = generos;

            // Historial del usuario con calificaciones
            var generosMejorReseñadoshistorial = _context.Historials
                .Where(h => h.UsuarioId == usuario && h.Calificacion > 2)
                .ToList();

            // Géneros del historial con sus respectivas calificaciones
            var generosHistorial = generosMejorReseñadoshistorial
                .SelectMany(h => h.Generos.Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(g => new { Genero = g.Trim(), h.Calificacion }));

            // Filtrar y obtener solo las calificaciones relacionadas con los géneros de la peli
            var calificacionesRelacionadas = generosHistorial
                .Where(gh => generoPeliculaElegida.Contains(gh.Genero))
                .Select(gh => gh.Calificacion)
                .ToList();

            // Calcular ajuste según el gusto histórico del usuario
            Console.WriteLine("calificaciones: " + string.Join(", ", calificacionesRelacionadas));
            if (calificacionesRelacionadas.Any())
            {
                var promedioGenero = calificacionesRelacionadas.Average(); // 0 a 5
                ajuste = (float)(promedioGenero / 5f); // Lo normalizamos a 0–1
            }
            
            return ajuste;
        }

    }
}
