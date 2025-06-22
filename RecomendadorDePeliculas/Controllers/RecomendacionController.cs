using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace RecomendadorDePeliculas.Web.Controllers
{

    public class RecomendacionController : Controller
    {
        private IRecomenderLogica _peliculaLogica;
        private ITmdbLogica _tmdbLogica;
        private readonly IRecomendadorPeliculasContext _context;
        private readonly IConfiguration _config;

        public RecomendacionController(IRecomenderLogica peliculaLogica, ITmdbLogica tmdbLogica, IRecomendadorPeliculasContext context, IConfiguration config)
        {
            _peliculaLogica = peliculaLogica;
            _tmdbLogica = tmdbLogica;
            _context = context;
            _config = config;
        }
        [HttpPost]
        public async Task<IActionResult> Recomendar([FromBody] RecomendacionRequest req)
        {
            int userId = int.Parse(HttpContext.Session.GetString("UserId"));
            float score = _peliculaLogica.RealizarPrediccionScore(userId, req.peliculaId);
            var imdbRating = await ObtenerPuntajeDesdeIMDB(req.titulo);
            float scoreFinal = score;
            var mensaje = InterpretarResultado(score);
            var titulo = req.titulo;
            return Json(new { titulo, mensaje, score, imdbRating });
        }

        private string InterpretarResultado(float score)
        {
            if (score >= 4) return "🟢 ¡Muy recomendable!";
            if (score >= 3) return "🟡 Puede gustarte";
            return "🔴 Poco probable que te guste";
        }

        private async Task<float> ObtenerPuntajeDesdeIMDB(string tituloPelicula)
        {
     
            var client = new HttpClient();
            var apiKey_settings = _config.GetSection("OMDB");
            var titulo_imdb = LimpiarTitulo(tituloPelicula);
            var apiKey = apiKey_settings["ApiKey"]; 
            var url = $"https://www.omdbapi.com/?t={titulo_imdb}&apikey={apiKey}";

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error consultando IMDb para '{tituloPelicula}': {response.StatusCode}");
                return 0f;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<OmdbResponse>(json);

            if (float.TryParse(data?.imdbRating, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rating))
            {
                return rating;
            }

            Console.WriteLine($"No se pudo obtener puntaje válido para '{tituloPelicula}'");
            return 0f;
        }
        public string LimpiarTitulo(string tituloConAnio)
        {
            var index = tituloConAnio.LastIndexOf('(');
            return index > 0 ? tituloConAnio.Substring(0, index).Trim() : tituloConAnio;
        }




    }
}
