using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        // =============================================
        // MÉTODOS DE EXPORTACIÓN
        // =============================================

        // GET: Reportes/ExportarPrestamosExcel
        [HttpGet]
        public IActionResult ExportarPrestamosExcel(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Configurar licencia de EPPlus (modo no comercial)
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var prestamos = ObtenerPrestamosPorPeriodo(fechaInicio, fechaFin);

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Préstamos");

                    // Título
                    worksheet.Cells["A1:I1"].Merge = true;
                    worksheet.Cells["A1"].Value = "REPORTE DE PRÉSTAMOS POR PERÍODO";
                    worksheet.Cells["A1"].Style.Font.Size = 16;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(13, 110, 253));
                    worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);

                    // Información del período
                    worksheet.Cells["A2"].Value = "Período:";
                    worksheet.Cells["B2"].Value = $"{fechaInicio?.ToString("dd/MM/yyyy") ?? "Inicio"} - {fechaFin?.ToString("dd/MM/yyyy") ?? "Fin"}";
                    worksheet.Cells["A3"].Value = "Fecha de generación:";
                    worksheet.Cells["B3"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                    // Encabezados
                    int row = 5;
                    worksheet.Cells[row, 1].Value = "ID";
                    worksheet.Cells[row, 2].Value = "Usuario";
                    worksheet.Cells[row, 3].Value = "Email";
                    worksheet.Cells[row, 4].Value = "Estado";
                    worksheet.Cells[row, 5].Value = "F. Préstamo";
                    worksheet.Cells[row, 6].Value = "F. Devolución";
                    worksheet.Cells[row, 7].Value = "F. Devuelto";
                    worksheet.Cells[row, 8].Value = "Días Atraso";
                    worksheet.Cells[row, 9].Value = "Materiales";

                    // Estilo de encabezados
                    using (var range = worksheet.Cells[row, 1, row, 9])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(211, 211, 211));
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Datos
                    row++;
                    foreach (var prestamo in prestamos)
                    {
                        worksheet.Cells[row, 1].Value = prestamo.TN_Id_Prestamo;
                        worksheet.Cells[row, 2].Value = prestamo.NombreUsuario;
                        worksheet.Cells[row, 3].Value = prestamo.TC_Email;
                        worksheet.Cells[row, 4].Value = prestamo.EstadoPrestamo;
                        worksheet.Cells[row, 5].Value = prestamo.TF_FechaPrestamo?.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 6].Value = prestamo.TF_FechaDevolucion?.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 7].Value = prestamo.TF_FechaDevuelto?.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 8].Value = prestamo.TN_DiasAtraso;
                        worksheet.Cells[row, 9].Value = prestamo.CantidadMateriales;

                        // Colorear filas con atraso
                        if (prestamo.TN_DiasAtraso > 0)
                        {
                            using (var range = worksheet.Cells[row, 1, row, 9])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 230, 230));
                            }
                        }

                        row++;
                    }

                    // Resumen
                    row++;
                    worksheet.Cells[row, 1].Value = "TOTAL PRÉSTAMOS:";
                    worksheet.Cells[row, 2].Value = prestamos.Count;
                    worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;

                    // Ajustar columnas
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Generar archivo
                    var fileName = $"Prestamos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var content = package.GetAsByteArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al exportar a Excel: " + ex.Message;
                return RedirectToAction("Prestamos");
            }
        }

        // GET: Reportes/ExportarPrestamosPDF
        [HttpGet]
        public IActionResult ExportarPrestamosPDF(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Configurar licencia de QuestPDF (modo comunitario)
                QuestPDF.Settings.License = LicenseType.Community;

                var prestamos = ObtenerPrestamosPorPeriodo(fechaInicio, fechaFin);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(QuestPDF.Helpers.Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header()
                            .Column(column =>
                            {
                                column.Item().Background(QuestPDF.Helpers.Colors.Blue.Medium)
                                    .Padding(10)
                                    .Text("REPORTE DE PRÉSTAMOS POR PERÍODO")
                                    .FontSize(18)
                                    .FontColor(QuestPDF.Helpers.Colors.White)
                                    .Bold()
                                    .AlignCenter();

                                column.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text($"Período: {fechaInicio?.ToString("dd/MM/yyyy") ?? "Inicio"} - {fechaFin?.ToString("dd/MM/yyyy") ?? "Fin"}");
                                    row.RelativeItem().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}");
                                });

                                column.Item().PaddingTop(5).LineHorizontal(1);
                            });

                        page.Content()
                            .PaddingVertical(10)
                            .Table(table =>
                            {
                                // Definir columnas
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);  // ID
                                    columns.RelativeColumn(2);   // Usuario
                                    columns.RelativeColumn(2);   // Email
                                    columns.RelativeColumn(1);   // Estado
                                    columns.RelativeColumn(1);   // F. Préstamo
                                    columns.RelativeColumn(1);   // F. Devolución
                                    columns.RelativeColumn(1);   // F. Devuelto
                                    columns.ConstantColumn(50);  // Días Atraso
                                    columns.ConstantColumn(60);  // Materiales
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("ID").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("Usuario").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("Email").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("Estado").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("F. Préstamo").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("F. Devolución").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("F. Devuelto").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("Días Atraso").Bold();
                                    header.Cell().Background(QuestPDF.Helpers.Colors.Grey.Lighten2).Padding(5).Text("Materiales").Bold();
                                });

                                // Datos
                                foreach (var prestamo in prestamos)
                                {
                                    var bgColor = prestamo.TN_DiasAtraso > 0 ? QuestPDF.Helpers.Colors.Red.Lighten4 : QuestPDF.Helpers.Colors.White;

                                    table.Cell().Background(bgColor).Padding(5).Text($"#{prestamo.TN_Id_Prestamo}");
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.NombreUsuario);
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.TC_Email).FontSize(8);
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.EstadoPrestamo);
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.TF_FechaPrestamo?.ToString("dd/MM/yyyy") ?? "-");
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.TF_FechaDevolucion?.ToString("dd/MM/yyyy") ?? "-");
                                    table.Cell().Background(bgColor).Padding(5).Text(prestamo.TF_FechaDevuelto?.ToString("dd/MM/yyyy") ?? "-");
                                    table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(prestamo.TN_DiasAtraso > 0 ? $"{prestamo.TN_DiasAtraso} días" : "-");
                                    table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(prestamo.CantidadMateriales.ToString());
                                }
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Página ");
                                x.CurrentPageNumber();
                                x.Span(" de ");
                                x.TotalPages();
                                x.Span(" - Biblioteca CTP Jicaral");
                            });
                    });
                });

                var fileName = $"Prestamos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al exportar a PDF: " + ex.Message;
                return RedirectToAction("Prestamos");
            }
        }

        // Método auxiliar para obtener préstamos por período
        private List<PrestamoPeriodo> ObtenerPrestamosPorPeriodo(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var prestamos = new List<PrestamoPeriodo>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamosPorPeriodo", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FechaInicio", (object)fechaInicio ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", (object)fechaFin ?? DBNull.Value);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        prestamos.Add(new PrestamoPeriodo
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

            return prestamos;
        }
    }
}