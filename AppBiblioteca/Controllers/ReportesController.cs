using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    [Authorize(Roles = "Bibliotecario,Administrador")]
    public class ReportesController : Controller
    {
        private readonly string _cs;

        public ReportesController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("DefaultConnection no configurada.");
        }

        // GET: Reportes/Dashboard
        [HttpGet]
        public IActionResult Dashboard()
        {
            try
            {
                var model = new DashboardViewModel();

                // Cargar estadísticas generales
                CargarEstadisticasGenerales(model);

                // Cargar datos para gráficos
                CargarPrestamosPorEstado(model);
                CargarPrestamosPorMes(model);
                CargarPrestamosPorCategoria(model);
                CargarTop10Materiales(model);
                CargarTop10Usuarios(model);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el dashboard: " + ex.Message;
                return View(new DashboardViewModel());
            }
        }

        // GET: Reportes/Prestamos
        [HttpGet]
        public IActionResult Prestamos()
        {
            var model = new ReportePrestamosViewModel
            {
                FechaInicio = DateTime.Now.AddDays(-30),
                FechaFin = DateTime.Now
            };
            return View(model);
        }

        // POST: Reportes/Prestamos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Prestamos(ReportePrestamosViewModel model)
        {
            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetPrestamosPorPeriodo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FechaInicio", (object)model.FechaInicio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaFin", (object)model.FechaFin ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Prestamos.Add(new PrestamoPeriodo
                            {
                                TN_Id_Prestamo = reader.GetInt32(reader.GetOrdinal("TN_Id_Prestamo")),
                                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                                TC_Email = reader.GetString(reader.GetOrdinal("TC_Email")),
                                EstadoPrestamo = reader.GetString(reader.GetOrdinal("EstadoPrestamo")),
                                TF_FechaPrestamo = reader.IsDBNull(reader.GetOrdinal("TF_FechaPrestamo")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaPrestamo")),
                                TF_FechaDevolucion = reader.IsDBNull(reader.GetOrdinal("TF_FechaDevolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaDevolucion")),
                                TF_FechaDevuelto = reader.IsDBNull(reader.GetOrdinal("TF_FechaDevuelto")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaDevuelto")),
                                TN_DiasAtraso = reader.GetInt32(reader.GetOrdinal("TN_DiasAtraso")),
                                CantidadMateriales = reader.GetInt32(reader.GetOrdinal("CantidadMateriales"))
                            });
                        }
                    }
                }

                TempData["SuccessMessage"] = $"Reporte generado: {model.TotalPrestamos} préstamo(s) encontrado(s).";
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al generar el reporte: " + ex.Message;
                return View(model);
            }
        }

        // GET: Reportes/Atrasos
        [HttpGet]
        public IActionResult Atrasos()
        {
            try
            {
                var model = new ReporteAtrasosViewModel();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetReporteAtrasos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.PrestamosAtrasados.Add(new PrestamoAtrasado
                            {
                                TN_Id_Prestamo = reader.GetInt32(reader.GetOrdinal("TN_Id_Prestamo")),
                                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                                TC_Email = reader.GetString(reader.GetOrdinal("TC_Email")),
                                TC_Telefono = reader.IsDBNull(reader.GetOrdinal("TC_Telefono")) ? null : reader.GetString(reader.GetOrdinal("TC_Telefono")),
                                TF_FechaPrestamo = reader.IsDBNull(reader.GetOrdinal("TF_FechaPrestamo")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaPrestamo")),
                                TF_FechaDevolucion = reader.IsDBNull(reader.GetOrdinal("TF_FechaDevolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaDevolucion")),
                                DiasAtraso = reader.GetInt32(reader.GetOrdinal("DiasAtraso")),
                                CantidadMateriales = reader.GetInt32(reader.GetOrdinal("CantidadMateriales"))
                            });
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el reporte: " + ex.Message;
                return View(new ReporteAtrasosViewModel());
            }
        }

        // GET: Reportes/Disponibilidad
        [HttpGet]
        public IActionResult Disponibilidad()
        {
            try
            {
                var model = new ReporteDisponibilidadViewModel();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetEstadisticasDisponibilidad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Categorias.Add(new DisponibilidadCategoria
                            {
                                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                                TotalMateriales = reader.GetInt32(reader.GetOrdinal("TotalMateriales")),
                                TotalEjemplares = reader.GetInt32(reader.GetOrdinal("TotalEjemplares")),
                                EjemplaresDisponibles = reader.GetInt32(reader.GetOrdinal("EjemplaresDisponibles")),
                                EjemplaresPrestados = reader.GetInt32(reader.GetOrdinal("EjemplaresPrestados")),
                                PorcentajeDisponibilidad = reader.GetDecimal(reader.GetOrdinal("PorcentajeDisponibilidad"))
                            });
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el reporte: " + ex.Message;
                return View(new ReporteDisponibilidadViewModel());
            }
        }

        // GET: Reportes/RecursosExternos
        [HttpGet]
        public IActionResult RecursosExternos()
        {
            try
            {
                var model = new ReporteRecursosExternosViewModel();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetReporteRecursosExternos", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Recursos.Add(new RecursoExternoPorAsignatura
                            {
                                Asignatura = reader.GetString(reader.GetOrdinal("Asignatura")),
                                TipoContenido = reader.IsDBNull(reader.GetOrdinal("TipoContenido")) ? null : reader.GetString(reader.GetOrdinal("TipoContenido")),
                                CantidadRecursos = reader.GetInt32(reader.GetOrdinal("CantidadRecursos")),
                                UltimaPublicacion = reader.IsDBNull(reader.GetOrdinal("UltimaPublicacion")) ? null : reader.GetDateTime(reader.GetOrdinal("UltimaPublicacion"))
                            });
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el reporte: " + ex.Message;
                return View(new ReporteRecursosExternosViewModel());
            }
        }

        // MÉTODOS PRIVADOS PARA CARGAR DATOS DEL DASHBOARD
        private void CargarEstadisticasGenerales(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetDashboardEstadisticas", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.TotalMateriales = reader.GetInt32(reader.GetOrdinal("TotalMateriales"));
                        model.TotalEjemplares = reader.GetInt32(reader.GetOrdinal("TotalEjemplares"));
                        model.EjemplaresDisponibles = reader.GetInt32(reader.GetOrdinal("EjemplaresDisponibles"));
                        model.TotalUsuarios = reader.GetInt32(reader.GetOrdinal("TotalUsuarios"));
                        model.TotalEstudiantes = reader.GetInt32(reader.GetOrdinal("TotalEstudiantes"));
                        model.PrestamosActivos = reader.GetInt32(reader.GetOrdinal("PrestamosActivos"));
                        model.ReservasPendientes = reader.GetInt32(reader.GetOrdinal("ReservasPendientes"));
                        model.PrestamosAtrasados = reader.GetInt32(reader.GetOrdinal("PrestamosAtrasados"));
                        model.TotalRecursosExternos = reader.GetInt32(reader.GetOrdinal("TotalRecursosExternos"));
                    }
                }
            }
        }

        private void CargarPrestamosPorEstado(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamosPorEstado", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.PrestamosPorEstado.Add(new PrestamosPorEstado
                        {
                            Estado = reader.GetString(reader.GetOrdinal("Estado")),
                            Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad"))
                        });
                    }
                }
            }
        }

        private void CargarPrestamosPorMes(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamosPorMes", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Meses", 12);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.PrestamosPorMes.Add(new PrestamosPorMes
                        {
                            Anio = reader.GetInt32(reader.GetOrdinal("Anio")),
                            Mes = reader.GetInt32(reader.GetOrdinal("Mes")),
                            NombreMes = reader.GetString(reader.GetOrdinal("NombreMes")),
                            TotalPrestamos = reader.GetInt32(reader.GetOrdinal("TotalPrestamos")),
                            Devueltos = reader.GetInt32(reader.GetOrdinal("Devueltos")),
                            Atrasados = reader.GetInt32(reader.GetOrdinal("Atrasados"))
                        });
                    }
                }
            }
        }

        private void CargarPrestamosPorCategoria(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamosPorCategoria", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", DBNull.Value);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.PrestamosPorCategoria.Add(new PrestamosPorCategoria
                        {
                            Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                            CantidadPrestamos = reader.GetInt32(reader.GetOrdinal("CantidadPrestamos"))
                        });
                    }
                }
            }
        }

        private void CargarTop10Materiales(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetTop10MaterialesPrestados", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", DBNull.Value);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Top10Materiales.Add(new MaterialTop
                        {
                            TN_Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                            TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                            TC_ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                            TC_Categoria = reader.GetString(reader.GetOrdinal("TC_Categoria")),
                            VecesPrestado = reader.GetInt32(reader.GetOrdinal("VecesPrestado")),
                            TN_Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles")),
                            TN_Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad"))
                        });
                    }
                }
            }
        }

        private void CargarTop10Usuarios(DashboardViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetTop10UsuariosActivos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", DBNull.Value);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Top10Usuarios.Add(new UsuarioTop
                        {
                            TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                            NombreCompleto = reader.GetString(reader.GetOrdinal("NombreCompleto")),
                            TC_Email = reader.GetString(reader.GetOrdinal("TC_Email")),
                            Rol = reader.GetString(reader.GetOrdinal("Rol")),
                            TotalPrestamos = reader.GetInt32(reader.GetOrdinal("TotalPrestamos")),
                            PrestamosActivos = reader.GetInt32(reader.GetOrdinal("PrestamosActivos")),
                            PrestamosAtrasados = reader.GetInt32(reader.GetOrdinal("PrestamosAtrasados"))
                        });
                    }
                }
            }
        }
    }
}