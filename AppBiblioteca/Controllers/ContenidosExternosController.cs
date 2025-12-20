using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    [Authorize(Roles = "Bibliotecario,Administrador")]
    public class ContenidosExternosController : Controller
    {
        private readonly string _cs;

        public ContenidosExternosController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("DefaultConnection no configurada.");
        }

        // GET: ContenidosExternos/Index
        [HttpGet]
        public IActionResult Index(int? filtroAsignatura, int? filtroTipoContenido, bool mostrarInactivos = false)
        {
            var model = new ContenidosExternosViewModel
            {
                FiltroAsignatura = filtroAsignatura,
                FiltroTipoContenido = filtroTipoContenido,
                MostrarInactivos = mostrarInactivos
            };

            try
            {
                // Cargar contenidos con filtros
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetContenidosExternos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Asignatura", (object)filtroAsignatura ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_TipoContenido", (object)filtroTipoContenido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Usuario", DBNull.Value);
                    cmd.Parameters.AddWithValue("@SoloActivos", !mostrarInactivos);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Contenidos.Add(new ContenidoExterno
                            {
                                TN_Id_Contenido = reader.GetInt32(reader.GetOrdinal("TN_Id_Contenido")),
                                TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                NombreProfesor = reader.GetString(reader.GetOrdinal("NombreProfesor")),
                                TN_Id_Asignatura = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")),
                                NombreAsignatura = reader.GetString(reader.GetOrdinal("NombreAsignatura")),
                                TN_Id_TipoContenido = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")),
                                TC_TipoContenido = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                                TC_Icono = reader.IsDBNull(reader.GetOrdinal("TC_Icono")) ? null : reader.GetString(reader.GetOrdinal("TC_Icono")),
                                TC_Descripcion = reader.IsDBNull(reader.GetOrdinal("TC_Descripcion")) ? null : reader.GetString(reader.GetOrdinal("TC_Descripcion")),
                                TC_URL = reader.GetString(reader.GetOrdinal("TC_URL")),
                                TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion")),
                                TF_FechaModificacion = reader.IsDBNull(reader.GetOrdinal("TF_FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaModificacion")),
                                TB_Activo = reader.GetBoolean(reader.GetOrdinal("TB_Activo"))
                            });
                        }
                    }
                }

                // Cargar listas para filtros
                LoadAsignaturas(model);
                LoadTiposContenido(model);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar contenidos: " + ex.Message;
                return View(model);
            }
        }

        // GET: ContenidosExternos/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new ContenidoExterno();
            LoadAsignaturas(model);
            LoadTiposContenido(model);
            return View(model);
        }

        // POST: ContenidosExternos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContenidoExterno model)
        {
            if (!ModelState.IsValid)
            {
                LoadAsignaturas(model);
                LoadTiposContenido(model);
                return View(model);
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_CreateContenidoExterno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Titulo", model.TC_Titulo);
                    cmd.Parameters.AddWithValue("@Id_Usuario", userId);
                    cmd.Parameters.AddWithValue("@Id_Asignatura", model.TN_Id_Asignatura);
                    cmd.Parameters.AddWithValue("@Id_TipoContenido", model.TN_Id_TipoContenido);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)model.TC_Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@URL", model.TC_URL);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Contenido externo creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear el contenido: " + ex.Message;
            }

            LoadAsignaturas(model);
            LoadTiposContenido(model);
            return View(model);
        }

        // GET: ContenidosExternos/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = new ContenidoExterno();

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetContenidoExternoById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Contenido", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.TN_Id_Contenido = reader.GetInt32(reader.GetOrdinal("TN_Id_Contenido"));
                            model.TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo"));
                            model.TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario"));
                            model.NombreProfesor = reader.GetString(reader.GetOrdinal("NombreProfesor"));
                            model.TN_Id_Asignatura = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura"));
                            model.TN_Id_TipoContenido = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido"));
                            model.TC_Descripcion = reader.IsDBNull(reader.GetOrdinal("TC_Descripcion")) ? null : reader.GetString(reader.GetOrdinal("TC_Descripcion"));
                            model.TC_URL = reader.GetString(reader.GetOrdinal("TC_URL"));
                            model.TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion"));
                            model.TB_Activo = reader.GetBoolean(reader.GetOrdinal("TB_Activo"));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Contenido no encontrado.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                LoadAsignaturas(model);
                LoadTiposContenido(model);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el contenido: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ContenidosExternos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ContenidoExterno model)
        {
            if (!ModelState.IsValid)
            {
                LoadAsignaturas(model);
                LoadTiposContenido(model);
                return View(model);
            }

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_UpdateContenidoExterno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Contenido", model.TN_Id_Contenido);
                    cmd.Parameters.AddWithValue("@Titulo", model.TC_Titulo);
                    cmd.Parameters.AddWithValue("@Id_Asignatura", model.TN_Id_Asignatura);
                    cmd.Parameters.AddWithValue("@Id_TipoContenido", model.TN_Id_TipoContenido);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)model.TC_Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@URL", model.TC_URL);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Contenido actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al actualizar el contenido: " + ex.Message;
            }

            LoadAsignaturas(model);
            LoadTiposContenido(model);
            return View(model);
        }

        // GET: ContenidosExternos/Details/5
        [HttpGet]
        [AllowAnonymous]  // Permitir a todos ver los detalles
        public IActionResult Details(int id)
        {
            try
            {
                ContenidoExterno model = null;

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetContenidoExternoById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Contenido", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model = new ContenidoExterno
                            {
                                TN_Id_Contenido = reader.GetInt32(reader.GetOrdinal("TN_Id_Contenido")),
                                TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                NombreProfesor = reader.GetString(reader.GetOrdinal("NombreProfesor")),
                                TN_Id_Asignatura = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")),
                                NombreAsignatura = reader.GetString(reader.GetOrdinal("NombreAsignatura")),
                                TN_Id_TipoContenido = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")),
                                TC_TipoContenido = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                                TC_Icono = reader.IsDBNull(reader.GetOrdinal("TC_Icono")) ? null : reader.GetString(reader.GetOrdinal("TC_Icono")),
                                TC_Descripcion = reader.IsDBNull(reader.GetOrdinal("TC_Descripcion")) ? null : reader.GetString(reader.GetOrdinal("TC_Descripcion")),
                                TC_URL = reader.GetString(reader.GetOrdinal("TC_URL")),
                                TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion")),
                                TF_FechaModificacion = reader.IsDBNull(reader.GetOrdinal("TF_FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaModificacion")),
                                TB_Activo = reader.GetBoolean(reader.GetOrdinal("TB_Activo"))
                            };
                        }
                    }
                }

                if (model == null)
                {
                    TempData["ErrorMessage"] = "Contenido no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el contenido: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: ContenidosExternos/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_DeleteContenidoExterno", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Contenido", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Contenido eliminado exitosamente.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al eliminar el contenido: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // Helper Methods
        private void LoadAsignaturas(ContenidoExterno model)
        {
            var items = new List<SelectListItem>();
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetAsignaturas", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("TC_Nombre")),
                            Selected = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")) == model.TN_Id_Asignatura
                        });
                    }
                }
            }
            model.AsignaturasList = items;
        }

        private void LoadAsignaturas(ContenidosExternosViewModel model)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Todas las asignaturas --" }
            };

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetAsignaturas", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("TC_Nombre")),
                            Selected = model.FiltroAsignatura.HasValue && reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")) == model.FiltroAsignatura.Value
                        });
                    }
                }
            }
            model.AsignaturasList = items;
        }

        private void LoadTiposContenido(ContenidoExterno model)
        {
            var items = new List<SelectListItem>();
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetTiposContenido", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                            Selected = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")) == model.TN_Id_TipoContenido
                        });
                    }
                }
            }
            model.TiposContenidoList = items;
        }

        private void LoadTiposContenido(ContenidosExternosViewModel model)
        {
            var items = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Todos los tipos --" }
            };

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetTiposContenido", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")).ToString(),
                            Text = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                            Selected = model.FiltroTipoContenido.HasValue && reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")) == model.FiltroTipoContenido.Value
                        });
                    }
                }
            }
            model.TiposContenidoList = items;
        }
    }
}