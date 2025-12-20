using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;
using System.Security.Claims;
using System.Diagnostics;

namespace AppBiblioteca.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AutoresController : Controller
    {
        private readonly string _cs;
        public AutoresController(IConfiguration config)
            => _cs = config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection no configurada.");


        // GET: Autores
        [HttpGet]
        public IActionResult Index()
        {
            var lista = new List<Autor>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetAutoresConPais", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new Autor
                {
                    Id_Autor = rdr.GetInt32(rdr.GetOrdinal("Id_Autor")),
                    Nombre = rdr.GetString(rdr.GetOrdinal("Nombre")),
                    Apellidos = rdr.GetString(rdr.GetOrdinal("Apellidos")),
                    Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais")),
                    NombrePais = rdr.GetString(rdr.GetOrdinal("NombrePais"))
                });
            }
            return View(lista);
        }


        // GET: Autores/Create
        [HttpGet]
        public IActionResult Create()
        {
            LoadPaises();
            return View();
        }

        // POST: Autores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Autor model)
        {


            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_RegisterAutor", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Apellidos", model.Apellidos);
                cmd.Parameters.AddWithValue("@Id_Pais", model.Id_Pais);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["SuccessMessage"] = "Autor registrado correctamente.";
                return RedirectToAction(nameof(Create));
            }
            catch (SqlException ex) when (ex.Number == 52001 || ex.Number == 52002)
            {
                TempData["ErrorMessage"] = ex.Message;
                LoadPaises();
                return View(model);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el autor.";
                LoadPaises();
                return View(model);
            }
        }

        // GET: Autores/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Autor model;
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetAutorById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Autor", id);

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound();

            model = new Autor
            {
                Id_Autor = rdr.GetInt32(rdr.GetOrdinal("Id_Autor")),
                Nombre = rdr.GetString(rdr.GetOrdinal("Nombre")),
                Apellidos = rdr.GetString(rdr.GetOrdinal("Apellidos")),
                Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais"))
            };

            LoadPaises();
            return View(model);
        }

        // POST: Autores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Autor model)
        {

            // 1) Validamos que la ruta y el modelo coincidan
            if (id != model.Id_Autor)
                return BadRequest();

            // 2) Si falla la validación de modelo, recargamos países y devolvemos la vista
            if (!ModelState.IsValid)
            {
                LoadPaises();
               // return View(model);
            }

            // 3) Antes de entrar al try, recargamos países por si hay error dentro del try
            LoadPaises();

            try
            {
               

                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_UpdateAutor", conn)

                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id_Autor", id);
                cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                cmd.Parameters.AddWithValue("@Apellidos", model.Apellidos);
                cmd.Parameters.AddWithValue("@Id_Pais", model.Id_Pais);

                conn.Open();
                cmd.ExecuteNonQuery();


                TempData["SuccessMessage"] = "Autor actualizado correctamente.";
                return RedirectToAction(nameof(Index));               

            }
            catch (SqlException ex) when (ex.Number == 52001 || ex.Number == 52002 || ex.Number == 52003)
            {
                // El SP lanzó “No se encontró…”
                TempData["ErrorMessage"] = ex.Message;
            }
            // En caso de otro error, o de validación de modelo:
            LoadPaises();
            return View(model);
        }




        /// <summary>
        /// Carga en ViewBag.Paises la lista de países para el dropdown.
        /// </summary>
        private void LoadPaises()
        {
            var lista = new List<(int Id, string Nombre)>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetPaises", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add((
                    rdr.GetInt32(rdr.GetOrdinal("Id_Pais")),
                    rdr.GetString(rdr.GetOrdinal("Pais"))
                ));
            }
            ViewBag.Paises = lista;
        }
    }
}