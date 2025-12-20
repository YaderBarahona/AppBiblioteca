using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    [Authorize]  // Accesible para todos los usuarios autenticados
    public class RecursosController : Controller
    {
        private readonly string _cs;

        public RecursosController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("DefaultConnection no configurada.");
        }

        // GET: Recursos/Index - Dashboard de recursos
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var model = new
                {
                    RecursosRecientes = GetRecursosRecientes(6),
                    Estadisticas = GetEstadisticas()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar recursos: " + ex.Message;
                return View();
            }
        }

        // GET: Recursos/Buscar - Búsqueda avanzada
        [HttpGet]
        public IActionResult Buscar()
        {
            var model = new BusquedaRecursosViewModel();
            LoadAsignaturas(model);
            LoadTiposContenido(model);
            return View(model);
        }

        // POST: Recursos/Buscar - Ejecutar búsqueda
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Buscar(BusquedaRecursosViewModel model)
        {
            model.HasSearched = true;

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_BuscarRecursosExternos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TextoBusqueda", (object)model.TextoBusqueda ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_Asignatura", (object)model.FiltroAsignatura ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NombreProfesor", (object)model.NombreProfesor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id_TipoContenido", (object)model.FiltroTipoContenido ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaDesde", (object)model.FechaDesde ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta", (object)model.FechaHasta ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Resultados.Add(new RecursoExterno
                            {
                                TN_Id_Contenido = reader.GetInt32(reader.GetOrdinal("TN_Id_Contenido")),
                                TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                NombreProfesor = reader.GetString(reader.GetOrdinal("NombreProfesor")),
                                EmailProfesor = reader.IsDBNull(reader.GetOrdinal("EmailProfesor")) ? null : reader.GetString(reader.GetOrdinal("EmailProfesor")),
                                TN_Id_Asignatura = reader.GetInt32(reader.GetOrdinal("TN_Id_Asignatura")),
                                NombreAsignatura = reader.GetString(reader.GetOrdinal("NombreAsignatura")),
                                CodigoAsignatura = reader.IsDBNull(reader.GetOrdinal("CodigoAsignatura")) ? null : reader.GetString(reader.GetOrdinal("CodigoAsignatura")),
                                TN_Id_TipoContenido = reader.GetInt32(reader.GetOrdinal("TN_Id_TipoContenido")),
                                TC_TipoContenido = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                                TC_Icono = reader.IsDBNull(reader.GetOrdinal("TC_Icono")) ? null : reader.GetString(reader.GetOrdinal("TC_Icono")),
                                TC_Descripcion = reader.IsDBNull(reader.GetOrdinal("TC_Descripcion")) ? null : reader.GetString(reader.GetOrdinal("TC_Descripcion")),
                                TC_URL = reader.GetString(reader.GetOrdinal("TC_URL")),
                                TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion")),
                                TF_FechaModificacion = reader.IsDBNull(reader.GetOrdinal("TF_FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaModificacion"))
                            });
                        }
                    }
                }

                LoadAsignaturas(model);
                LoadTiposContenido(model);

                if (model.HasResults)
                {
                    TempData["SuccessMessage"] = $"Se encontraron {model.TotalResultados} recurso(s).";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al buscar recursos: " + ex.Message;
                LoadAsignaturas(model);
                LoadTiposContenido(model);
                return View(model);
            }
        }

        // GET: Recursos/PorAsignatura/5
        [HttpGet]
        public IActionResult PorAsignatura(int id)
        {
            try
            {
                var recursos = new List<RecursoExterno>();
                string nombreAsignatura = "";

                // Obtener nombre de la asignatura
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("SELECT TC_Nombre FROM TCTPB_Cat_Asignaturas WHERE TN_Id_Asignatura = @Id", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    conn.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        nombreAsignatura = result.ToString();
                }

                // Obtener recursos
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetRecursosPorAsignatura", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Asignatura", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            recursos.Add(new RecursoExterno
                            {
                                TN_Id_Contenido = reader.GetInt32(reader.GetOrdinal("TN_Id_Contenido")),
                                TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                NombreProfesor = reader.GetString(reader.GetOrdinal("NombreProfesor")),
                                TC_TipoContenido = reader.GetString(reader.GetOrdinal("TC_TipoContenido")),
                                TC_Icono = reader.IsDBNull(reader.GetOrdinal("TC_Icono")) ? null : reader.GetString(reader.GetOrdinal("TC_Icono")),
                                TC_Descripcion = reader.IsDBNull(reader.GetOrdinal("TC_Descripcion")) ? null : reader.GetString(reader.GetOrdinal("TC_Descripcion")),
                                TC_URL = reader.GetString(reader.GetOrdinal("TC_URL")),
                                TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion")),
                                NombreAsignatura = nombreAsignatura
                            });
                        }
                    }
                }

                ViewBag.NombreAsignatura = nombreAsignatura;
                return View(recursos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar recursos: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Recursos/Ver/5 - Ver detalles de un recurso
        [HttpGet]
        public IActionResult Ver(int id)
        {
            try
            {
                RecursoExterno recurso = null;

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
                            recurso = new RecursoExterno
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
                                TF_FechaModificacion = reader.IsDBNull(reader.GetOrdinal("TF_FechaModificacion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaModificacion"))
                            };
                        }
                    }
                }

                if (recurso == null)
                {
                    TempData["ErrorMessage"] = "Recurso no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                return View(recurso);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el recurso: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper Methods
        private List<RecursoExterno> GetRecursosRecientes(int cantidad)
        {
            var recursos = new List<RecursoExterno>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetRecursosRecientes", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        recursos.Add(new RecursoExterno
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
                            TF_FechaCreacion = reader.GetDateTime(reader.GetOrdinal("TF_FechaCreacion"))
                        });
                    }
                }
            }

            return recursos;
        }

        private EstadisticasRecursosViewModel GetEstadisticas()
        {
            var estadisticas = new EstadisticasRecursosViewModel();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetEstadisticasRecursos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    // Primer resultado: Total
                    if (reader.Read())
                        estadisticas.TotalRecursos = reader.GetInt32(0);

                    // Segundo resultado: Por asignatura
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            estadisticas.PorAsignatura.Add(new AsignaturaEstadistica
                            {
                                Nombre = reader.GetString(0),
                                Cantidad = reader.GetInt32(1)
                            });
                        }
                    }

                    // Tercer resultado: Por profesor
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            estadisticas.PorProfesor.Add(new ProfesorEstadistica
                            {
                                Nombre = reader.GetString(0),
                                Cantidad = reader.GetInt32(1)
                            });
                        }
                    }

                    // Cuarto resultado: Por tipo
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            estadisticas.PorTipo.Add(new TipoEstadistica
                            {
                                Tipo = reader.GetString(0),
                                Cantidad = reader.GetInt32(1)
                            });
                        }
                    }
                }
            }

            return estadisticas;
        }

        private void LoadAsignaturas(BusquedaRecursosViewModel model)
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

        private void LoadTiposContenido(BusquedaRecursosViewModel model)
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