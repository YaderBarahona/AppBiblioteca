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
    public class PaisesController : Controller
    {
        private readonly string _cs;
        public PaisesController(IConfiguration config)
            => _cs = config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection no configurada.");

        // GET: Paises
        public IActionResult Index()
        {
            var lista = new List<Pais>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetPaises", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                lista.Add(new Pais
                {
                    Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais")),
                    NombrePais = rdr.GetString(rdr.GetOrdinal("Pais"))
                });
            }

            return View(lista);
        }


        // GET: Paises/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: Paises/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pais model)
        {
            if (!ModelState.IsValid) return View(model);
            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_RegisterPais", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Pais", model.NombrePais);
                conn.Open();
                cmd.ExecuteNonQuery();
                TempData["SuccessMessage"] = "País agregado.";
                return RedirectToAction(nameof(Create));
            }
            catch (SqlException ex) when (ex.Number == 51000)
            {
                // mostrará "Ese país ya está registrado."
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }

        // GET: Paises/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Pais model = null;
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetPaisById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Pais", id);

            conn.Open();
            using var rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                model = new Pais
                {
                    Id_Pais = rdr.GetInt32(rdr.GetOrdinal("Id_Pais")),
                    NombrePais = rdr.GetString(rdr.GetOrdinal("Pais"))
                };
            }

            if (model == null)
                return NotFound();

            return View(model);
        }

        // POST: Paises/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Pais model)
        {
            if (id != model.Id_Pais) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            try
            {
                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_UpdatePais", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id_Pais", id);
                cmd.Parameters.AddWithValue("@Pais", model.NombrePais);
                conn.Open();
                cmd.ExecuteNonQuery();
                TempData["SuccessMessage"] = "País actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex) when (ex.Number == 51001)
            {
                // mostrará "Ya existe un país con ese nombre."
                TempData["ErrorMessage"] = ex.Message;
                return View(model);
            }
        }
    }
}