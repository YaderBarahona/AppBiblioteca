using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;

namespace AppBiblioteca.Controllers
{
    [Authorize(Roles = "Bibliotecario,Administrador")]
    public class ConsultasController : Controller
    {
        private readonly string _cs;

        public ConsultasController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Falta DefaultConnection");
        }

        // GET: Consultas/Index
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Consultas/BuscarMateriales
        [HttpGet]
        public IActionResult BuscarMateriales()
        {
            var model = new BusquedaMaterialesViewModel();
            CargarListasMateriales(model);
            return View(model);
        }

        // POST: Consultas/BuscarMateriales
        [HttpPost]
        public IActionResult BuscarMateriales(BusquedaMaterialesViewModel model)
        {
            try
            {
                model.HasSearched = true;
                model.Resultados = new List<Material>();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_BuscarMateriales", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Titulo", (object)model.Titulo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ISBN", (object)model.ISBN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdCategoria", (object)model.IdCategoria ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdEditorial", (object)model.IdEditorial ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdAutor", (object)model.IdAutor ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Estado", (object)model.Estado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AnioDesde", (object)model.AnioDesde ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AnioHasta", (object)model.AnioHasta ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Resultados.Add(new Material
                            {
                                Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                                Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                                CategoriaNombre = reader.GetString(reader.GetOrdinal("Categoria")),
                                EditorialNombre = reader.GetString(reader.GetOrdinal("Editorial")),
                                AnioPublicacion = reader.GetInt32(reader.GetOrdinal("AnioPublicacion")),
                                Idioma = reader.GetString(reader.GetOrdinal("TC_Idioma")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad")),
                                Pretados = reader.GetInt32(reader.GetOrdinal("TN_Prestados")),
                                Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles")),
                                Estado = reader.GetString(reader.GetOrdinal("TC_Estado")),
                                Autores = reader.IsDBNull(reader.GetOrdinal("Autores")) ? "" : reader.GetString(reader.GetOrdinal("Autores"))
                            });
                        }
                    }
                }

                if (!model.HasResults)
                {
                    TempData["InfoMessage"] = "No se encontraron materiales con los criterios especificados.";
                }

                CargarListasMateriales(model);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al buscar materiales: " + ex.Message;
                CargarListasMateriales(model);
                return View(model);
            }
        }

        // GET: Consultas/BuscarUsuarios
        [HttpGet]
        public IActionResult BuscarUsuarios()
        {
            var model = new BusquedaUsuariosViewModel();
            CargarListasUsuarios(model);
            return View(model);
        }

        // POST: Consultas/BuscarUsuarios
        [HttpPost]
        public IActionResult BuscarUsuarios(BusquedaUsuariosViewModel model)
        {
            try
            {
                model.HasSearched = true;
                model.Resultados = new List<UsuarioConsulta>();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_BuscarUsuarios", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserName", (object)model.UserName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", (object)model.Email ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdRol", (object)model.IdRol ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Activo", (object)model.Activo ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Resultados.Add(new UsuarioConsulta
                            {
                                TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                TC_UserName = reader.GetString(reader.GetOrdinal("TC_UserName")),
                                TC_Email = reader.GetString(reader.GetOrdinal("TC_Email")),
                                NombreRol = reader.GetString(reader.GetOrdinal("NombreRol")),
                                TN_Id_Rol = reader.GetInt32(reader.GetOrdinal("TN_Id_Rol")),
                                TB_Activo = reader.GetBoolean(reader.GetOrdinal("TB_Activo")),
                                PrestamosActivos = reader.GetInt32(reader.GetOrdinal("PrestamosActivos")),
                                TotalPrestamos = reader.GetInt32(reader.GetOrdinal("TotalPrestamos"))
                            });
                        }
                    }
                }

                if (!model.HasResults)
                {
                    TempData["InfoMessage"] = "No se encontraron usuarios con los criterios especificados.";
                }

                CargarListasUsuarios(model);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al buscar usuarios: " + ex.Message;
                CargarListasUsuarios(model);
                return View(model);
            }
        }

        // GET: Consultas/HistorialPrestamos
        [HttpGet]
        public IActionResult HistorialPrestamos()
        {
            var model = new HistorialPrestamosViewModel();
            CargarListasHistorial(model);
            return View(model);
        }

        // POST: Consultas/HistorialPrestamos
        [HttpPost]
        public IActionResult HistorialPrestamos(HistorialPrestamosViewModel model)
        {
            try
            {
                model.HasSearched = true;
                model.Resultados = new List<PrestamoHistorial>();

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_HistorialPrestamosGeneral", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdUsuario", (object)model.IdUsuario ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdEstado", (object)model.IdEstado ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaDesde", (object)model.FechaDesde ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaHasta", (object)model.FechaHasta ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConAtraso", (object)model.ConAtraso ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Resultados.Add(new PrestamoHistorial
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
                                TN_DiasAtraso = reader.GetInt32(reader.GetOrdinal("TN_DiasAtraso")),
                                CantidadMateriales = reader.GetInt32(reader.GetOrdinal("CantidadMateriales"))
                            });
                        }
                    }
                }

                if (!model.HasResults)
                {
                    TempData["InfoMessage"] = "No se encontraron préstamos con los criterios especificados.";
                }

                CargarListasHistorial(model);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al buscar historial: " + ex.Message;
                CargarListasHistorial(model);
                return View(model);
            }
        }

        // GET: Consultas/Estadisticas
        [HttpGet]
        public IActionResult Estadisticas()
        {
            var model = new EstadisticasViewModel();

            try
            {
                using (var conn = new SqlConnection(_cs))
                {
                    conn.Open();

                    // Estadísticas de materiales
                    using (var cmd = new SqlCommand("usp_EstadisticasMateriales", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model.TotalMateriales = reader.GetInt32(reader.GetOrdinal("TotalMateriales"));
                                model.TotalEjemplares = reader.GetInt32(reader.GetOrdinal("TotalEjemplares"));
                                model.TotalDisponibles = reader.GetInt32(reader.GetOrdinal("TotalDisponibles"));
                                model.TotalPrestados = reader.GetInt32(reader.GetOrdinal("TotalPrestados"));
                                model.MaterialesDisponibles = reader.GetInt32(reader.GetOrdinal("MaterialesDisponibles"));
                                model.MaterialesDadosBaja = reader.GetInt32(reader.GetOrdinal("MaterialesDadosBaja"));
                            }
                        }
                    }

                    // Estadísticas de préstamos
                    using (var cmd = new SqlCommand("usp_EstadisticasPrestamos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model.TotalPrestamos = reader.GetInt32(reader.GetOrdinal("TotalPrestamos"));
                                model.Reservados = reader.GetInt32(reader.GetOrdinal("Reservados"));
                                model.Prestados = reader.GetInt32(reader.GetOrdinal("Prestados"));
                                model.Devueltos = reader.GetInt32(reader.GetOrdinal("Devueltos"));
                                model.Cancelados = reader.GetInt32(reader.GetOrdinal("Cancelados"));
                                model.ConAtraso = reader.GetInt32(reader.GetOrdinal("ConAtraso"));
                            }
                        }
                    }

                    // Materiales más prestados
                    using (var cmd = new SqlCommand("usp_MaterialesMasPrestados", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Top", 10);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.MaterialesMasPrestados.Add(new MaterialMasPrestado
                                {
                                    TN_Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                                    TC_Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                    TC_ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                                    TC_Categoria = reader.GetString(reader.GetOrdinal("TC_Categoria")),
                                    VecesPrestado = reader.GetInt32(reader.GetOrdinal("VecesPrestado")),
                                    TN_Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles")),
                                    TN_Prestados = reader.GetInt32(reader.GetOrdinal("TN_Prestados"))
                                });
                            }
                        }
                    }

                    // Usuarios más activos
                    using (var cmd = new SqlCommand("usp_UsuariosMasActivos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Top", 10);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.UsuariosMasActivos.Add(new UsuarioMasActivo
                                {
                                    TN_Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario")),
                                    TC_UserName = reader.GetString(reader.GetOrdinal("TC_UserName")),
                                    TC_Email = reader.GetString(reader.GetOrdinal("TC_Email")),
                                    TC_Role = reader.GetString(reader.GetOrdinal("TC_Role")),
                                    TotalPrestamos = reader.GetInt32(reader.GetOrdinal("TotalPrestamos")),
                                    PrestamosActivos = reader.GetInt32(reader.GetOrdinal("PrestamosActivos")),
                                    PrestamosAtrasados = reader.GetInt32(reader.GetOrdinal("PrestamosAtrasados"))
                                });
                            }
                        }
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar estadísticas: " + ex.Message;
                return View(model);
            }
        }

        // Métodos auxiliares privados
        private void CargarListasMateriales(BusquedaMaterialesViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                // Cargar categorías
                var categorias = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todas --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Categoria, TC_Categoria FROM TCTPB_Cat_Categorias ORDER BY TC_Categoria", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categorias.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Categorias = categorias;

                // Cargar editoriales
                var editoriales = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todas --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Editoriales, TC_Editorial FROM TCTPB_Cat_Editoriales ORDER BY TC_Editorial", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        editoriales.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Editoriales = editoriales;

                // Cargar autores
                var autores = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todos --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Autor, TC_Nombre + ' ' + TC_Apellidos AS NombreCompleto FROM TCTPB_Cat_Autores ORDER BY TC_Apellidos, TC_Nombre", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        autores.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Autores = autores;
            }
        }

        private void CargarListasUsuarios(BusquedaUsuariosViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                // Cargar roles
                var roles = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todos --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Role, TC_Role FROM TCTPB_Cat_Roles ORDER BY TC_Role", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roles.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Roles = roles;
            }
        }

        private void CargarListasHistorial(HistorialPrestamosViewModel model)
        {
            using (var conn = new SqlConnection(_cs))
            {
                conn.Open();

                // Cargar usuarios
                var usuarios = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todos --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Usuario, TC_UserName FROM TCTPB_Reg_Usuarios ORDER BY TC_UserName", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usuarios.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Usuarios = usuarios;

                // Cargar estados
                var estados = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Todos --" } };
                using (var cmd = new SqlCommand("SELECT TN_Id_Estado, TC_Estado FROM TCTPB_Cat_EstadoPrestamo ORDER BY TN_Id_Estado", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        estados.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
                model.Estados = estados;
            }
        }
    }
}