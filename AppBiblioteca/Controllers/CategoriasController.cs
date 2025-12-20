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
    [Authorize(Roles = "Administrador")]
    //[Authorize(Roles = "Bibliotecario,Administrador")]
    public class CategoriasController : Controller
    {
        private readonly string _cs;
        public CategoriasController(IConfiguration config)
            => _cs = config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection no configurada.");

        // GET: Categorias
        public IActionResult Index()
        {
            var lista = new List<Categoria>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetCategorias", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new Categoria
                {
                    Id_Categoria = rdr.GetInt32(rdr.GetOrdinal("Id_Categoria")),
                    CategoriaNombre = rdr.GetString(rdr.GetOrdinal("CategoriaNombre")),
                    Descripcion = rdr.GetString(rdr.GetOrdinal("Descripcion"))
                });
            }

            return View(lista);
        }


        // GET: Categoria/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_RegisterCategoria", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Categoria", model.CategoriaNombre);
                cmd.Parameters.AddWithValue("@Descripcion", model.Descripcion);

                conn.Open();
                cmd.ExecuteNonQuery();
                TempData["SuccessMessage"] = "Categoría agregada.";
                return RedirectToAction(nameof(Create));
            }
            catch (SqlException ex) when (ex.Number == 54001)
            {
                // Ya existe una categoría con ese nombre."
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al crear la categoría.";
                return View(model);
            }
        }

        // GET: Categorias/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Categoria model = null;
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetCategoriaById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Categoria", id);

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                model = new Categoria
                {
                    Id_Categoria = rdr.GetInt32(rdr.GetOrdinal("Id_Categoria")),
                    CategoriaNombre = rdr.GetString(rdr.GetOrdinal("CategoriaNombre")),
                    Descripcion = rdr.GetString(rdr.GetOrdinal("Descripcion"))
                };
            }

            if (model == null)
                return NotFound();

            return View(model);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Categoria model)
        {
            if (id != model.Id_Categoria) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_UpdateCategoria", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@Id_Categoria", id);
                cmd.Parameters.AddWithValue("@Categoria", model.CategoriaNombre);
                cmd.Parameters.AddWithValue("@Descripcion", model.Descripcion);
                conn.Open();
                cmd.ExecuteNonQuery();

                //mensaje en ventana para mostrar a completar el proceso
                TempData["SuccessMessage"] = "Categoría actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex) when (ex.Number == 54101 || ex.Number == 54102)
            {
                // mostrará "Ya existe ua categoria con ese nombre."
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }
    }
}