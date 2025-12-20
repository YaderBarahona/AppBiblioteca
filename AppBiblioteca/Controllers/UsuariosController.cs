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
    [Authorize(Roles = "Bibliotecario,Administrador")]
    public class UsuariosController : Controller
    {
        private readonly string _cs;

        public UsuariosController(IConfiguration config)
        {
            _cs = config.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Falta la cadena 'DefaultConnection' en la configuración.");
        }




        //Muestra usuarios activos = 1, el sp maneja los permisos
        // GET: Usuarios/Index
        [HttpGet]
        public IActionResult UsuariosTrue()
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var listaUsuarios = new List<Usuario>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetUsuariosForUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CurrentRole", currentRole);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listaUsuarios.Add(new Usuario
                {
                    Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Cedula = reader.GetString(reader.GetOrdinal("Cedula")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Seccion = reader.IsDBNull(reader.GetOrdinal("Seccion"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("Seccion")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("Telefono")),
                    Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                    Role = reader.GetString(reader.GetOrdinal("Role"))
                });
            }

            return View(listaUsuarios);
        }

        //Muestra usuarios dados de baja = 0, el sp maneja los permisos
        // GET: Usuarios/UsuariosFalse
        [HttpGet]
        public IActionResult UsuariosFalse()
        {
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var listaUsuarios = new List<Usuario>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetUsuarios_False", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CurrentRole", currentRole);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listaUsuarios.Add(new Usuario
                {
                    Id_Usuario = reader.GetInt32(reader.GetOrdinal("Id_Usuario")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Cedula = reader.GetString(reader.GetOrdinal("Cedula")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Seccion = reader.IsDBNull(reader.GetOrdinal("Seccion"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("Seccion")),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("Telefono"))
                                  ? null
                                  : reader.GetString(reader.GetOrdinal("Telefono")),
                    Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                    Role = reader.GetString(reader.GetOrdinal("Role"))
                });
            }

            return View(listaUsuarios);
        }








        // GET: Usuarios/Create
        [HttpGet]
        public IActionResult Create()
        {

            LoadRolesIntoViewBag();
            return View();
        }



        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario model, int selectedRoleId)
        {

            // LoadRolesIntoViewBag();  // <- Re-cargar al devolver la vista



            try
            {

                using var conn = new SqlConnection(_cs);
                using var cmd = new SqlCommand("usp_RegisterUsuario", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Parámetros obligatorios
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Password", model.Password);
                cmd.Parameters.AddWithValue("@UserName", model.UserName);
                cmd.Parameters.AddWithValue("@LastName", model.LastName);
                cmd.Parameters.AddWithValue("@Cedula", model.Cedula);

                // Parámetros opcionales
                cmd.Parameters.AddWithValue("@Seccion", (object?)model.Seccion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Telefono", (object?)model.Telefono ?? DBNull.Value);


                // Parámetro del rol
                cmd.Parameters.AddWithValue("@Id_Role", selectedRoleId);






                conn.Open();
                cmd.ExecuteNonQuery();

                // Si llegamos aquí, todo fue bien:
                TempData["SuccessMessage"] = "Usuario registrado con éxito.";
                // Redirige a la misma Create para que la vista muestre la alerta
                return RedirectToAction(nameof(Create));
            }
            catch (SqlException ex) when (ex.Number == 51000)
            {
                // SP lanzó: 'Ya existe un usuario con la misma cédula.'
                TempData["ErrorMessage"] = ex.Message;
                LoadRolesIntoViewBag();
                return View(model);
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al crear el usuario.";

            }
            // En caso de error, recargar roles y volver a la vista
            LoadRolesIntoViewBag();
            return View(model);
        }







        /// <summary>
        /// Carga en ViewBag.Roles la lista de roles permitidos
        /// según el rol del usuario actual (Bibliotecario o Administrador).
        /// </summary>
        private void LoadRolesIntoViewBag()
        {
            var currentRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "";
            var roles = new List<(int Id, string Name)>();

            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetRolesForUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@CurrentRole", currentRole);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                roles.Add((
                    reader.GetInt32(reader.GetOrdinal("Id_Role")),
                    reader.GetString(reader.GetOrdinal("Role"))
                ));
            }

            ViewBag.Roles = roles;
        }







        /// <summary>
        /// pasa los usuarios false a true (inactivos a activos)
        /// solo este rol puede hacer la accion ( Administrador).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActivo(int id)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_ToggleActivoUsuario", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Usuario", id);

            conn.Open();
            cmd.ExecuteNonQuery();

            // Si llegamos aquí, todo fue bien:
            TempData["SuccessMessage"] = "Usuario bloqueado.";
            return RedirectToAction(nameof(UsuariosTrue));
        }


        // GET: Usuarios/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            try
            {
                var usuario = new Usuario();

                // Obtener información del usuario
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
                    SELECT
                        u.TN_Id_Usuario,
                        u.TC_UserName,
                        u.TC_LastName,
                        u.TC_Cedula,
                        u.TC_Email,
                        u.TC_Seccion,
                        u.TC_Telefono,
                        u.TB_Activo,
                        r.TC_Role
                    FROM TCTPB_Reg_Usuarios u
                    INNER JOIN TCTPB_Reg_UsuarioRoles ur ON u.TN_Id_Usuario = ur.TN_Id_Usuario
                    INNER JOIN TCTPB_Cat_Roles r ON ur.TN_Id_Role = r.TN_Id_Role
                    WHERE u.TN_Id_Usuario = @Id_Usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@Id_Usuario", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario.Id_Usuario = reader.GetInt32(reader.GetOrdinal("TN_Id_Usuario"));
                            usuario.UserName = reader.GetString(reader.GetOrdinal("TC_UserName"));
                            usuario.LastName = reader.GetString(reader.GetOrdinal("TC_LastName"));
                            usuario.Cedula = reader.GetString(reader.GetOrdinal("TC_Cedula"));
                            usuario.Email = reader.GetString(reader.GetOrdinal("TC_Email"));
                            usuario.Seccion = reader.IsDBNull(reader.GetOrdinal("TC_Seccion")) ? null : reader.GetString(reader.GetOrdinal("TC_Seccion"));
                            usuario.Telefono = reader.IsDBNull(reader.GetOrdinal("TC_Telefono")) ? null : reader.GetString(reader.GetOrdinal("TC_Telefono"));
                            usuario.Activo = reader.GetBoolean(reader.GetOrdinal("TB_Activo"));
                            usuario.Role = reader.GetString(reader.GetOrdinal("TC_Role"));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Usuario no encontrado.";
                            return RedirectToAction(nameof(UsuariosTrue));
                        }
                    }
                }

                // Obtener estadísticas de préstamos del usuario
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
                    SELECT
                        COUNT(CASE WHEN TN_Id_Estado = 2 THEN 1 END) AS PrestamosActivos,
                        COUNT(*) AS TotalPrestamos,
                        COUNT(CASE WHEN TN_Id_Estado = 2 AND GETDATE() > TF_FechaDevolucion THEN 1 END) AS PrestamosAtrasados
                    FROM TCTPB_Reg_Prestamos
                    WHERE TN_Id_Usuario = @Id_Usuario", conn))
                {
                    cmd.Parameters.AddWithValue("@Id_Usuario", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ViewBag.PrestamosActivos = reader.GetInt32(0);
                            ViewBag.TotalPrestamos = reader.GetInt32(1);
                            ViewBag.PrestamosAtrasados = reader.GetInt32(2);
                        }
                    }
                }

                return View(usuario);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el usuario: " + ex.Message;
                return RedirectToAction(nameof(UsuariosTrue));
            }
        }



        /// <summary>
        /// pasa los usuarios true a false (activos a inactivos)
        /// solo este rol puede hacer la accion ( Administrador).
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFalse(int id)
        {
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_ToggleActivoUsuario", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Id_Usuario", id);

            conn.Open();
            cmd.ExecuteNonQuery();

            // Si llegamos aquí, todo fue bien:
            TempData["SuccessMessage"] = "Usuario desbloqueado.";
            return RedirectToAction(nameof(UsuariosFalse));
        }








    }
}
