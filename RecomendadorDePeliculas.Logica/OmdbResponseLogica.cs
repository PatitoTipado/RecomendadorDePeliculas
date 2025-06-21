using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RecomendadorDePeliculas.Logica
{
    public class OmdbResponse
    {
        [JsonPropertyName("imdbRating")]
        public string imdbRating { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; }

        // Agregá más propiedades si necesitás
    }

}
