using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;


namespace RecomendadorDePeliculas.Web.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioLogica _usuarioLogica;

        public UsuarioController(IUsuarioLogica usuarioLogica)
        {
            _usuarioLogica = usuarioLogica;
        }

        [HttpGet]
        public IActionResult Editar()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Login");

            var usuario = _usuarioLogica.ObtenerPorId(userId);

            var modelo = new UsuarioEditarViewModel
            {
                Id = usuario.Id,
                Correo = usuario.Correo,
                FechaDeNacimiento = usuario.FechaDeNacimiento,
                Genero = usuario.Genero
            };

            CargarGeneros();

            return View(modelo);
        }

        [HttpPost]
        public IActionResult Editar(UsuarioEditarViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                CargarGeneros();
                return View(modelo);
            }

            try
            {
                var usuario = _usuarioLogica.ObtenerPorId(modelo.Id);

                usuario.Correo = modelo.Correo;
                usuario.FechaDeNacimiento = modelo.FechaDeNacimiento;
                usuario.Genero = modelo.Genero;

                if (!string.IsNullOrWhiteSpace(modelo.Contrasenia))
                {
                    var hasher = new PasswordHasher<Usuario>();
                    usuario.ContraseniaHash = hasher.HashPassword(usuario, modelo.Contrasenia);
                }

                _usuarioLogica.Actualizar(usuario);

                TempData["aviso"] = "Perfil actualizado correctamente";
                return RedirectToAction("Generos", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarGeneros();
                return View(modelo);
            }
        }

        private void CargarGeneros()
        {
            ViewBag.Generos = new List<SelectListItem>
            {
                new SelectListItem { Text = "Femenino", Value = "F" },
                new SelectListItem { Text = "Masculino", Value = "M" },
                new SelectListItem { Text = "Otro", Value = "O" }
            };
        }
    }
}
