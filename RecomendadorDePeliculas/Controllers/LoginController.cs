using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RecomendadorDePeliculas.Entidades.Models;
using RecomendadorDePeliculas.Logica;
using RecomendadorDePeliculas.Web.Services;
using System.Security.Claims;


namespace RecomendadorDePeliculas.Web.Controllers
{
    public class LoginController : Controller
    {
        private IUsuarioLogica _usuarioLogica;
        private readonly EmailService _emailService;
        public LoginController(IUsuarioLogica usuarioLogica, EmailService emailService)
        {
            _usuarioLogica = usuarioLogica;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarLogin(string correo,string contrasenia)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            if (_usuarioLogica.ValidarLogin(correo, contrasenia))
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, correo),
            };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                int id = _usuarioLogica.obtenerIdUsuarioPorCorreo(correo);

                HttpContext.Session.SetString("UserId", id.ToString());

                return Redirect("/Home/CalificarPeliculas");
            }

            TempData["Mensaje"] = "Correo o contraseña incorrecta";
            return View("login");
        }

        public async Task <IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            return View(new Usuario());
        }

        [HttpPost]
        public IActionResult Registrar(Usuario usuario,string rcontra)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

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

            TempData["aviso"] = "Registro Exitoso";
            
            return View("login");
        }

        [HttpGet]
        public IActionResult RecuperarContrasenia()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ReestablecerContrasenia(string token)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            var usuario = _usuarioLogica.ObtenerPorToken(token);

            if (usuario == null || usuario.TokenExpiracion < DateTime.Now)
            {
                TempData["Mensaje"] = "El enlace es inválido o ha expirado.";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ReestablecerContrasenia(string token, string nuevaContrasenia, string repetirContrasenia)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            if (nuevaContrasenia != repetirContrasenia)
            {
                ViewBag.Token = token;
                TempData["Mensaje"] = "Las contraseñas no coinciden.";
                return View();
            }

            var usuario = _usuarioLogica.ObtenerPorToken(token);

            if (usuario == null || usuario.TokenExpiracion < DateTime.Now)
            {
                TempData["Mensaje"] = "El enlace es inválido o ha expirado.";
                return RedirectToAction("Login");
            }

            var hasher = new PasswordHasher<Usuario>();
            usuario.ContraseniaHash = hasher.HashPassword(usuario, nuevaContrasenia);
            usuario.TokenRecuperacion = null;
            usuario.TokenExpiracion = null;

            _usuarioLogica.Actualizar(usuario);

            TempData["aviso"] = "Contraseña actualizada correctamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult RecuperarContrasenia(string correo)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return Redirect("/Home/CalificarPeliculas");
            }

            var usuario = _usuarioLogica.ObtenerPorCorreo(correo);
            if (usuario != null)
            {
                var token = Guid.NewGuid().ToString();
                usuario.TokenRecuperacion = token;
                usuario.TokenExpiracion = DateTime.Now.AddHours(1); 

                _usuarioLogica.Actualizar(usuario); 

                var enlace = Url.Action("ReestablecerContrasenia", "Login", new { token = token }, Request.Scheme);

                string mensaje = $@"
            <h2>Recuperación de contraseña</h2>
            <p>Haz clic en el siguiente enlace para restablecer tu contraseña:</p>
            <p><a href='{enlace}'>Restablecer contraseña</a></p>
            <p>Este enlace será válido por 1 hora.</p>";

                _emailService.EnviarEmail(usuario.Correo, "Recuperación de contraseña", mensaje);
            }

            TempData["aviso"] = "Si el correo está registrado, recibirás un mensaje pronto. Verifica tu bandeja de entrada.";
            return View("login");
        }

    }
}
