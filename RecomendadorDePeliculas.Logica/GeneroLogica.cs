
using RecomendadorDePeliculas.Entidades.Models;

namespace RecomendadorDePeliculas.Logica
{
    public interface IGeneroLogica
    {
        public bool noPoseeGeneros(int idUsuario);
        List<string> ObtenerGenerosDePeliculasFavoritas(int userId);
        string[] ObtenerGenerosPreferidos(int userId);
        List<GeneroPelicula> ObtenerTodosLosGeneros();
        public List<int> ObtenerGenerosFavoritos(int userId);
        void GuardarPreferencias(int userId,List<int> generosSeleccionados);
    }
    public class GeneroLogica : IGeneroLogica
    {
        private IRecomendadorPeliculasContext _context;

        public GeneroLogica(IRecomendadorPeliculasContext context)
        {
            _context = context;
        }

        public List<int> ObtenerGenerosFavoritos(int userId)
        {
            return _context.UsuarioGeneros
                .Where(ug => ug.UsuarioId == userId)
                .Select(ug => ug.GeneroId)
                .ToList();
        }

        public bool noPoseeGeneros(int idUsuario)
        {
            return _context.UsuarioGeneros.FirstOrDefault(g=> g.UsuarioId==idUsuario) ==null;
        }

        public List<string> ObtenerGenerosDePeliculasFavoritas(int userId)
        {
            return _context.Historials
                        .Where(h => h.UsuarioId == userId && h.Calificacion > 3)
                        .Select(h => h.Generos)
                        .ToList();
        }

        public List<GeneroPelicula> ObtenerTodosLosGeneros()
        {
            return _context.GenerosPeliculas.ToList();
        }
        public string[] ObtenerGenerosPreferidos(int userId)
        {
            return _context.UsuarioGeneros
                .Where(ug => ug.UsuarioId == userId)
                .Select(ug => ug.Genero.Nombre)
                .ToArray();
        }

        public void GuardarPreferencias(int userId, List<int> generosSeleccionados)
        {
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
        }
    }
}
