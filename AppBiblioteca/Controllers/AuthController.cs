using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    public class AuthController : Controller
    {
        private readonly string _cs;
        private readonly PasswordHasher<string> _hasher = new();

        public AuthController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Falta la cadena 'DefaultConnection' en configuración.");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            int userId;
            string storedPassword, userName, role;

            // Llamada al stored procedure usp_LoginUsuario
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_LoginUsuario", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email });

                conn.Open();
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    ModelState.AddModelError("", "Comunícate con la biblioteca.");
                    return View();
                }

                // Leer columnas por nombre
                userId = reader.GetInt32(reader.GetOrdinal("Id_Usuario"));
                storedPassword = reader.GetString(reader.GetOrdinal("Password"));
                userName = reader.GetString(reader.GetOrdinal("UserName"));
                role = reader.GetString(reader.GetOrdinal("Role"));
            }

            // Verificar contraseña
            if (storedPassword != password)
            {
                ModelState.AddModelError("", "Email o contraseña inválida.");
                return View();
            }


            // Crear claims e iniciar cookie
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name,            userName),
                new Claim(ClaimTypes.Role,            role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            ).Wait();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
