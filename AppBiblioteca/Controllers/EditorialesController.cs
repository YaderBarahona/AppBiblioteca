using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;
using System.Security.Claims;

namespace AppBiblioteca.Controllers
{
    //[Authorize(Roles = "Bibliotecario,Administrador")]
    [Authorize(Roles = "Administrador")]
    public class EditorialesController : Controller
    {
        private readonly string _cs;
        public EditorialesController(IConfiguration config)
            => _cs = config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection no configurada.");

        // GET: Editoriales
        [HttpGet]
        public IActionResult Index()
        {
            var lista = new List<Editorial>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetEditorialesConPais", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new Editorial
                {
                    Id_Editorial = rdr.GetInt32(rdr.GetOrdinal("Id_Editoriales")),
                    EditorialNombre = rdr.GetString(rdr.GetOrdinal("EditorialNombre")),
                    Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais")),
                    NombrePais = rdr.GetString(rdr.GetOrdinal("NombrePais"))
                });
            }

            return View(lista);
        }



        // GET: Editoriales/Create
        [HttpGet]
        public IActionResult Create()
        {
            LoadPaises();
            return View();
        }

        // POST: Editoriales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Editorial model)
        {

            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_RegisterEditorial", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Editorial", model.EditorialNombre);
                cmd.Parameters.AddWithValue("@Id_Pais", model.Id_Pais);

                conn.Open();
                cmd.ExecuteNonQuery();

                TempData["SuccessMessage"] = "Editorial creada correctamente.";
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
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el editorial.";
                LoadPaises();
                return View(model);
            }
        }


        // GET: Editorial/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Editorial model;
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetEditorialById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Editoriales", id);

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            if (!rdr.Read())
                return NotFound();

            model = new Editorial
            {
                Id_Editorial= rdr.GetInt32(rdr.GetOrdinal("Id_Editorial")),
                EditorialNombre = rdr.GetString(rdr.GetOrdinal("EditorialNombre")),              
                Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais"))
            };

            LoadPaises();
            return View(model);
        }


        // POST: Editoriales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Editorial model)
        {

            // 1) Validamos que la ruta y el modelo coincidan
            if (id != model.Id_Editorial)
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
                using var cmd = new SqlCommand("usp_UpdateEditorial", conn)

                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id_Editoriales", id);
                cmd.Parameters.AddWithValue("@Editorial", model.EditorialNombre);
                cmd.Parameters.AddWithValue("@Id_Pais", model.Id_Pais);

                conn.Open();
                cmd.ExecuteNonQuery();


                TempData["SuccessMessage"] = "Editorial actualizado correctamente.";
                return RedirectToAction(nameof(Index));

            }
            catch (SqlException ex) when (ex.Number == 53001 || ex.Number == 53002 || ex.Number == 53003)
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