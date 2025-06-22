using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecomendadorDePeliculas.Entidades.Models;

namespace RecomendadorDePeliculas.Logica
{
    public interface IUsuarioLogica
    {
        int obtenerIdUsuarioPorCorreo(string correo);
        void Registrar(Usuario usuario);
        bool ValidarLogin(string correo, string contrasenia);
        Usuario ObtenerPorId(int id);
        void Actualizar(Usuario usuario);
        Usuario ObtenerPorCorreo(string correo);
        Usuario ObtenerPorToken(string token);


        // ✅ Nuevo método
        bool CorreoEnUsoPorOtroUsuario(int idUsuarioActual, string correo);
    }

    public class UsuarioLogica : IUsuarioLogica
    {
        private readonly RecomendadorPeliculasContext _context;

        public UsuarioLogica(RecomendadorPeliculasContext context)
        {
            _context = context;
        }

        public int obtenerIdUsuarioPorCorreo(string correo)
        {
            Usuario usuario = _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo).Result;
            return usuario?.Id ?? 0;
        }

        public void Registrar(Usuario usuario)
        {
            string contraseniaEnTextoPlano = usuario.ContraseniaHash;
            var passwordHasher = new PasswordHasher<Usuario>();
            usuario.ContraseniaHash = passwordHasher.HashPassword(usuario, contraseniaEnTextoPlano);

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
        }

        public bool ValidarLogin(string correo, string contrasenia)
        {
            Usuario usuario = _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo).Result;

            if (usuario == null)
                return false;

            var passwordHasher = new PasswordHasher<Usuario>();
            var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.ContraseniaHash, contrasenia);

            return resultado == PasswordVerificationResult.Success;
        }

        public Usuario ObtenerPorId(int id)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Id == id);
        }

        public void Actualizar(Usuario usuario)
        {
            var usuarioExistente = _context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (usuarioExistente != null)
            {
                if (!string.IsNullOrWhiteSpace(usuario.Correo))
                    usuarioExistente.Correo = usuario.Correo;

                if (usuario.FechaDeNacimiento.HasValue)
                    usuarioExistente.FechaDeNacimiento = usuario.FechaDeNacimiento;

                if (!string.IsNullOrWhiteSpace(usuario.Genero))
                    usuarioExistente.Genero = usuario.Genero;

                if (!string.IsNullOrWhiteSpace(usuario.ContraseniaHash))
                {
                    usuarioExistente.ContraseniaHash = usuario.ContraseniaHash;
                }

                _context.SaveChanges();
            }
        }

        // ✅ Nuevo método para validación desde el ViewModel
        public bool CorreoEnUsoPorOtroUsuario(int idUsuarioActual, string correo)
        {
            return _context.Usuarios
                .Any(u => u.Correo == correo && u.Id != idUsuarioActual);
        }

        public Usuario ObtenerPorCorreo(string correo)
        {
            return _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
        }

        public Usuario ObtenerPorToken(string token)
        {
            return _context.Usuarios.FirstOrDefault(u => u.TokenRecuperacion == token);
        }

    }
}

