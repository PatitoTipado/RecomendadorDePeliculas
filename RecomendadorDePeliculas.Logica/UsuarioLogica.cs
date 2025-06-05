using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecomendadorDePeliculas.Entidades.Models;

namespace RecomendadorDePeliculas.Logica
{

    public interface IUsuarioLogica
    {
        public void Registrar(Usuario usuario);

        public bool ValidarLogin(string correo, string contrasenia);

    }

    public class UsuarioLogica : IUsuarioLogica
    {

        private readonly RecomendadorPeliculasContext _context;

        public UsuarioLogica(RecomendadorPeliculasContext context)
        {
            _context= context;
        }

        public void Registrar(Usuario usuario)
        {
            string contraseniaEnTextoPlano = usuario.ContraseniaHash;
            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.ContraseniaHash = passwordHasher.HashPassword(usuario, contraseniaEnTextoPlano);

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }
       
        public bool ValidarLogin(string correo,string contrasenia)
        {

            Usuario usuario = _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo).Result;

            if (usuario == null)
                return false;

            var passwordHasher = new PasswordHasher<Usuario>();
            var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.ContraseniaHash, contrasenia);

            return resultado == PasswordVerificationResult.Success;
        }
    }
}
