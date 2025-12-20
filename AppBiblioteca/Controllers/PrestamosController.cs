using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;
using AppBiblioteca.Services;

namespace AppBiblioteca.Controllers
{
    [Authorize]
    public class PrestamosController : Controller
    {
        private readonly string _cs;
        private readonly IEmailService _emailService;

        public PrestamosController(IConfiguration config, IEmailService emailService)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Falta DefaultConnection");
            _emailService = emailService;
        }

        // GET: Prestamos/Index
        [HttpGet]
        public IActionResult Index()
        {
            var prestamos = new List<Prestamo>();

            try
            {
                // Determinar filtros según rol
                int? idUsuario = null;
                int? idEstado = null;

                if (User.IsInRole("Estudiante"))
                {
                    // Estudiantes solo ven sus propios préstamos
                    idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                prestamos = GetPrestamosConDetalles(idUsuario, idEstado);

                // Pasar información de rol al ViewBag
                ViewBag.EsBibliotecario = User.IsInRole("Bibliotecario") || User.IsInRole("Administrador");
                ViewBag.EsEstudiante = User.IsInRole("Estudiante");

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar préstamos: " + ex.Message;
                return View(prestamos);
            }
        }

        // GET: Prestamos/CrearReserva
        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        public IActionResult CrearReserva()
        {
            var model = new CrearReservaViewModel();

            try
            {
                // Obtener materiales disponibles
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetMaterialesDisponibles", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.MaterialesDisponibles.Add(new MaterialDisponible
                            {
                                TN_Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                                TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                TC_ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                                TN_Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles")),
                                Categoria = reader.GetString(reader.GetOrdinal("Categoria")),
                                Autores = reader.IsDBNull(reader.GetOrdinal("Autores")) ? "" : reader.GetString(reader.GetOrdinal("Autores"))
                            });
                        }
                    }
                }

                if (!model.MaterialesDisponibles.Any())
                {
                    TempData["InfoMessage"] = "No hay materiales disponibles para reservar en este momento.";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar materiales: " + ex.Message;
                return View(model);
            }
        }

        // POST: Prestamos/CrearReserva
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Estudiante")]
        public async System.Threading.Tasks.Task<IActionResult> CrearReserva(CrearReservaViewModel model)
        {
            if (!ModelState.IsValid || !model.MaterialesSeleccionados.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un material.";
                return RedirectToAction(nameof(CrearReserva));
            }

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var materialesIds = string.Join(",", model.MaterialesSeleccionados);
                int nuevoPrestamoId = 0;

                // Obtener datos del usuario desde la base de datos
                string userName = "";
                string userEmail = "";
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("SELECT TC_UserName, TC_Email FROM TCTPB_Reg_Usuarios WHERE TN_Id_Usuario = @UserId", conn))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userName = reader.GetString(reader.GetOrdinal("TC_UserName"));
                            userEmail = reader.GetString(reader.GetOrdinal("TC_Email"));
                        }
                    }
                }

                // Crear la reserva y obtener el ID
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_CrearReserva", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Usuario", userId);
                    cmd.Parameters.AddWithValue("@MaterialesIds", materialesIds);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nuevoPrestamoId = reader.GetInt32(reader.GetOrdinal("NuevoPrestamoId"));
                        }
                    }
                }

                // Obtener títulos de los materiales seleccionados para la notificación
                var materialesTitulos = new List<string>();
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("SELECT TC_Titulo FROM TCTPB_Cat_Materiales WHERE TN_Id_Material IN (" + materialesIds + ")", conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            materialesTitulos.Add(reader.GetString(0));
                        }
                    }
                }

                // Enviar notificación por email
                if (!string.IsNullOrEmpty(userEmail))
                {
                    await _emailService.SendReservaConfirmationAsync(
                        userEmail,
                        userName,
                        nuevoPrestamoId,
                        materialesTitulos.ToArray()
                    );
                }

                TempData["SuccessMessage"] = "Reserva creada exitosamente. Espere la aprobación del bibliotecario.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(CrearReserva));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al crear la reserva: " + ex.Message;
                return RedirectToAction(nameof(CrearReserva));
            }
        }

        // GET: Prestamos/MisPrestamos
        [HttpGet]
        [Authorize(Roles = "Estudiante")]
        public IActionResult MisPrestamos()
        {
            var prestamos = new List<Prestamo>();

            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                prestamos = GetPrestamosConDetalles(userId, null);

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar sus préstamos: " + ex.Message;
                return View(prestamos);
            }
        }

        // GET: Prestamos/PendientesAprobacion
        [HttpGet]
        [Authorize(Roles = "Bibliotecario,Administrador")]
        public IActionResult PendientesAprobacion()
        {
            var prestamos = new List<Prestamo>();

            try
            {
                // Estado 1 = Reservado
                prestamos = GetPrestamosConDetalles(null, 1);

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar reservas pendientes: " + ex.Message;
                return View(prestamos);
            }
        }

        // GET: Prestamos/Activos
        [HttpGet]
        [Authorize(Roles = "Bibliotecario,Administrador")]
        public IActionResult Activos()
        {
            var prestamos = new List<Prestamo>();

            try
            {
                // Estado 2 = Prestado
                prestamos = GetPrestamosConDetalles(null, 2);

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar préstamos activos: " + ex.Message;
                return View(prestamos);
            }
        }

        // POST: Prestamos/Aprobar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario,Administrador")]
        public async System.Threading.Tasks.Task<IActionResult> Aprobar(int id)
        {
            try
            {
                // Obtener información del préstamo antes de aprobarlo
                var prestamo = GetPrestamosConDetalles(null, null)
                    .FirstOrDefault(p => p.TN_Id_Prestamo == id);

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_AprobarPrestamo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Prestamo", id);
                    cmd.Parameters.AddWithValue("@DiasPrestamo", 7); // 7 días de préstamo

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Enviar notificación al usuario
                if (prestamo != null && !string.IsNullOrEmpty(prestamo.EmailUsuario))
                {
                    var fechaDevolucion = DateTime.Now.AddDays(7);
                    var materialesTitulos = prestamo.Detalles
                        .Select(d => d.TituloMaterial)
                        .ToArray();

                    await _emailService.SendReservaAprobadaAsync(
                        prestamo.EmailUsuario,
                        prestamo.NombreUsuario,
                        id,
                        materialesTitulos,
                        fechaDevolucion
                    );
                }

                TempData["SuccessMessage"] = "Préstamo aprobado exitosamente.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al aprobar el préstamo: " + ex.Message;
            }

            return RedirectToAction(nameof(PendientesAprobacion));
        }

        // POST: Prestamos/Devolver
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Bibliotecario,Administrador")]
        public async System.Threading.Tasks.Task<IActionResult> Devolver(int id)
        {
            try
            {
                // Obtener información del préstamo antes de devolverlo
                var prestamo = GetPrestamosConDetalles(null, 2) // Estado 2 = Prestado
                    .FirstOrDefault(p => p.TN_Id_Prestamo == id);

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_DevolverPrestamo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Prestamo", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Enviar notificación al usuario
                if (prestamo != null && !string.IsNullOrEmpty(prestamo.EmailUsuario))
                {
                    var materialesTitulos = prestamo.Detalles
                        .Select(d => d.TituloMaterial)
                        .ToArray();

                    await _emailService.SendDevolucionConfirmadaAsync(
                        prestamo.EmailUsuario,
                        prestamo.NombreUsuario,
                        id,
                        materialesTitulos
                    );
                }

                TempData["SuccessMessage"] = "Devolución registrada exitosamente.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al registrar la devolución: " + ex.Message;
            }

            return RedirectToAction(nameof(Activos));
        }

        // POST: Prestamos/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Cancelar(int id)
        {
            try
            {
                // Obtener información del préstamo antes de cancelarlo
                var prestamo = GetPrestamosConDetalles(null, 1) // Estado 1 = Reservado
                    .FirstOrDefault(p => p.TN_Id_Prestamo == id);

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_CancelarPrestamo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Prestamo", id);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                // Enviar notificación al usuario
                if (prestamo != null && !string.IsNullOrEmpty(prestamo.EmailUsuario))
                {
                    var motivo = User.IsInRole("Estudiante")
                        ? "Cancelación solicitada por el usuario"
                        : "Cancelación realizada por el bibliotecario";

                    await _emailService.SendReservaCanceladaAsync(
                        prestamo.EmailUsuario,
                        prestamo.NombreUsuario,
                        id,
                        motivo
                    );
                }

                TempData["SuccessMessage"] = "Reserva cancelada exitosamente.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cancelar la reserva: " + ex.Message;
            }

            if (User.IsInRole("Estudiante"))
                return RedirectToAction(nameof(MisPrestamos));
            else
                return RedirectToAction(nameof(Index));
        }

        // GET: Prestamos/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var prestamo = GetPrestamosConDetalles(null, null)
                    .FirstOrDefault(p => p.TN_Id_Prestamo == id);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "Préstamo no encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Verificar que el usuario solo puede ver sus propios préstamos (si es estudiante)
                if (User.IsInRole("Estudiante"))
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    if (prestamo.TN_Id_Usuario != userId)
                    {
                        TempData["ErrorMessage"] = "No tiene permiso para ver este préstamo.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                return View(prestamo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el préstamo: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Prestamos/ConAtraso
        [HttpGet]
        [Authorize(Roles = "Bibliotecario,Administrador")]
        public IActionResult ConAtraso()
        {
            var prestamos = new List<Prestamo>();

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetPrestamosConAtraso", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var prestamo = new Prestamo
                            {
                                TN_Id_Prestamo = reader.GetInt32(reader.GetOrdinal("TN_Id_Prestamo")),
                                TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                                EmailUsuario = reader.GetString(reader.GetOrdinal("EmailUsuario")),
                                TF_FechaPrestamo = reader.GetDateTime(reader.GetOrdinal("TF_FechaPrestamo")),
                                TF_FechaDevolucion = reader.GetDateTime(reader.GetOrdinal("TF_FechaDevolucion")),
                                TN_DiasAtraso = reader.GetInt32(reader.GetOrdinal("DiasAtrasoActual")),
                                TN_Id_Estado = 2, // Prestado
                                EstadoNombre = "Prestado"
                            };

                            // Cargar detalles
                            prestamo.Detalles = GetDetallesPrestamo(prestamo.TN_Id_Prestamo);
                            prestamos.Add(prestamo);
                        }
                    }
                }

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar préstamos con atraso: " + ex.Message;
                return View(prestamos);
            }
        }

        // Métodos auxiliares privados
        private List<Prestamo> GetPrestamosConDetalles(int? idUsuario, int? idEstado)
        {
            var prestamos = new List<Prestamo>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamos", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Usuario", (object)idUsuario ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id_Estado", (object)idEstado ?? DBNull.Value);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var prestamo = new Prestamo
                        {
                            TN_Id_Prestamo = reader.GetInt32(reader.GetOrdinal("TN_Id_Prestamo")),
                            TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                            NombreUsuario = reader.GetString(reader.GetOrdinal("NombreUsuario")),
                            EmailUsuario = reader.GetString(reader.GetOrdinal("EmailUsuario")),
                            TN_Id_Estado = reader.GetInt32(reader.GetOrdinal("TN_Id_Estado")),
                            EstadoNombre = reader.GetString(reader.GetOrdinal("EstadoNombre")),
                            TF_FechaPrestamo = reader.IsDBNull(reader.GetOrdinal("TF_FechaPrestamo")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaPrestamo")),
                            TF_FechaDevolucion = reader.IsDBNull(reader.GetOrdinal("TF_FechaDevolucion")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaDevolucion")),
                            TF_FechaDevuelto = reader.IsDBNull(reader.GetOrdinal("TF_FechaDevuelto")) ? null : reader.GetDateTime(reader.GetOrdinal("TF_FechaDevuelto")),
                            TN_DiasAtraso = reader.GetInt32(reader.GetOrdinal("TN_DiasAtraso"))
                        };

                        prestamos.Add(prestamo);
                    }
                }
            }

            // Cargar detalles para cada préstamo
            foreach (var prestamo in prestamos)
            {
                prestamo.Detalles = GetDetallesPrestamo(prestamo.TN_Id_Prestamo);
            }

            return prestamos;
        }

        private List<PrestamoDetalle> GetDetallesPrestamo(int idPrestamo)
        {
            var detalles = new List<PrestamoDetalle>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetPrestamoDetalles", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id_Prestamo", idPrestamo);

                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        detalles.Add(new PrestamoDetalle
                        {
                            TN_Id_Detalle = reader.GetInt32(reader.GetOrdinal("TN_Id_Detalle")),
                            TN_Id_Prestamo = reader.GetInt32(reader.GetOrdinal("TN_Id_Prestamo")),
                            TN_Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                            TituloMaterial = reader.GetString(reader.GetOrdinal("TituloMaterial")),
                            ISBN = reader.GetString(reader.GetOrdinal("ISBN")),
                            Autores = reader.IsDBNull(reader.GetOrdinal("Autores")) ? "" : reader.GetString(reader.GetOrdinal("Autores"))
                        });
                    }
                }
            }

            return detalles;
        }
    }
}