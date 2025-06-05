using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace RecomendadorDePeliculas.Web.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private IUsuarioLogica _usuarioLogica;
        public LoginController(IUsuarioLogica usuarioLogica) {
        
            _usuarioLogica= usuarioLogica;

        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarLogin(string correo,string contrasenia)
        {
            if (_usuarioLogica.ValidarLogin(correo,contrasenia))
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, correo),
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return Redirect("/Home/Index");
            }

            TempData["Mensaje"] = "Correo o contraseña incorrecta";
            return View("login");
        }

        [HttpPost]
        public async Task <IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("Login");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View(new Usuario());
        }

        [HttpPost]
        public IActionResult Registrar(Usuario usuario,string rcontra)
        {
            if (!usuario.ContraseniaHash.Equals(rcontra))
            {
                TempData["rcontra"] = "las contraseñas no coinciden";
                return View(usuario);
            }

            if (!ModelState.IsValid)
            {
                return View(usuario);
            }

            _usuarioLogica.Registrar(usuario);

            TempData["aviso"] = "se registro correctamente el usuario";
            
            return View("login");
        }

        [HttpGet]
        public IActionResult RecuperarContrasenia()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecuperarContrasenia(string correo)
        {
            //enviar correo si esta logeado
            TempData["aviso"] = "Si el correo está registrado, recibirás un mensaje pronto verifique su correo";
            return View("login");
        }
    }
}
