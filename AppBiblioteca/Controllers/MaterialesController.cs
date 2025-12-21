using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using AppBiblioteca.Models;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class MaterialesController : Controller
    {
        private readonly string _cs;
        public MaterialesController(IConfiguration config)
            => _cs = config.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("DefaultConnection no configurada.");




        // GET: Materiales/Index
        [HttpGet]
        public IActionResult Index(string estadoFiltro = "Todos")
        {
            var materiales = new List<Material>();

            using (var conn = new SqlConnection(_cs))
            using (var cmd = new SqlCommand("usp_GetMaterialesConAutores", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        materiales.Add(new Material
                        {
                            Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                            Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                            ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                            CategoriaNombre = reader.GetString(reader.GetOrdinal("Categoria")),
                            AnioPublicacion = reader.GetInt32(reader.GetOrdinal("TN_FechaPublicacion")),
                            Idioma = reader.GetString(reader.GetOrdinal("TC_Idioma")),
                            Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad")),
                            Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles")),
                            EditorialNombre = reader.GetString(reader.GetOrdinal("Editorial")),
                            Estado = reader.GetString(reader.GetOrdinal("TC_Estado")),
                            Autores = reader.GetString(reader.GetOrdinal("Autores"))
                        });
                    }
                }
            }

            // Filtrar por estado si no es "Todos"
            if (estadoFiltro != "Todos")
            {
                materiales = materiales.Where(m => m.Estado == estadoFiltro).ToList();
            }

            // Pasar el filtro actual al ViewBag
            ViewBag.EstadoFiltro = estadoFiltro;

            return View(materiales);
        }



        // GET: Materiales/Create
        [HttpGet]
        public IActionResult Create()
        {
            var model = new Material();
            LoadCategorias(model);
            LoadEditoriales(model);
            LoadAutores(model);
            LoadAnios();

            return View(model);
        }

        // POST: Materiales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Material model)
        {
            if (!ModelState.IsValid)
            {
                LoadCategorias(model);
                LoadEditoriales(model);
                LoadAutores(model);
                LoadAnios();
                return View(model);
            }

            try
            {
                int newId;
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_RegisterMaterial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Titulo", model.Titulo);
                    cmd.Parameters.AddWithValue("@ISBN", model.ISBN);
                    cmd.Parameters.AddWithValue("@Id_Categoria", model.Id_Categoria);
                    cmd.Parameters.AddWithValue("@FechaPublicacion", model.AnioPublicacion);
                    cmd.Parameters.AddWithValue("@Idioma", model.Idioma);
                    cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                    cmd.Parameters.AddWithValue("@Disponibles", model.Disponibles);
                    cmd.Parameters.AddWithValue("@Id_Editorial", model.Id_Editorial);
                    cmd.Parameters.AddWithValue("@Estado", model.Estado);

                    // Parámetro de salida
                    var pOut = new SqlParameter("@NewId_Material", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(pOut);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    newId = (int)pOut.Value;
                }

                // Luego, para cada autor seleccionado, insertamos en la relación
                using (var conn = new SqlConnection(_cs))
                {
                    conn.Open();
                    foreach (var autorId in model.SelectedAutorIds)
                    {
                        using var cmd2 = new SqlCommand("usp_AddAutorToMaterial", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd2.Parameters.AddWithValue("@Id_Material", newId);
                        cmd2.Parameters.AddWithValue("@Id_Autor", autorId);
                        cmd2.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Material y autores guardados correctamente.";
                return RedirectToAction(nameof(Create));
            }
            catch (SqlException ex) when (
                   ex.Number == 55001
                || ex.Number == 55002
                || ex.Number == 55003)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch
            {
                TempData["ErrorMessage"] = "Ocurrió un error al registrar el material.";
            }

            LoadCategorias(model);
            LoadEditoriales(model);
            LoadAutores(model);
            LoadAnios();

            return View(model);
        }







        private void LoadCategorias(Material model)
        {
            var items = new List<SelectListItem>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetCategorias", conn) { CommandType = CommandType.StoredProcedure };
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                items.Add(new SelectListItem
                {
                    Value = rdr.GetInt32(rdr.GetOrdinal("Id_Categoria")).ToString(),
                    Text = rdr.GetString(rdr.GetOrdinal("CategoriaNombre")),
                    Selected = (rdr.GetInt32(rdr.GetOrdinal("Id_Categoria")) == model.Id_Categoria)
                });
            model.CategoryList = items;
        }

        private void LoadEditoriales(Material model)
        {
            var items = new List<SelectListItem>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetEditorialesConPais", conn) { CommandType = CommandType.StoredProcedure };
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
                items.Add(new SelectListItem
                {
                    Value = rdr.GetInt32(rdr.GetOrdinal("Id_Editoriales")).ToString(),
                    Text = rdr.GetString(rdr.GetOrdinal("EditorialNombre")),
                    Selected = (rdr.GetInt32(rdr.GetOrdinal("Id_Editoriales")) == model.Id_Editorial)
                });
            model.EditorialList = items;
        }

        private void LoadAutores(Material model)
        {
            var items = new List<SelectListItem>();
            using var conn = new SqlConnection(_cs);
            using var cmd = new SqlCommand("usp_GetAutoresConPais", conn) { CommandType = CommandType.StoredProcedure };
            conn.Open();
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                var id = rdr.GetInt32(rdr.GetOrdinal("Id_Autor"));
                items.Add(new SelectListItem
                {
                    Value = id.ToString(),
                    Text = $"{rdr.GetString(rdr.GetOrdinal("Apellidos"))}, {rdr.GetString(rdr.GetOrdinal("Nombre"))}",
                    Selected = model.SelectedAutorIds.Contains(id)
                });
            }
            model.AutorList = items;
        }

        private void LoadAnios()
        {

            int añoActual = DateTime.Now.Year;
            var años = Enumerable
                .Range(1900, añoActual - 1900 + 1)
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                })
                .ToList();

            ViewBag.Years = años;
        }



        // GET: Materiales/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var model = new Material();

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetMaterialById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material"));
                            model.Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo"));
                            model.ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN"));
                            model.Id_Categoria = reader.GetInt32(reader.GetOrdinal("TN_Id_Categoria"));
                            model.AnioPublicacion = reader.GetInt32(reader.GetOrdinal("TN_FechaPublicacion"));
                            model.Idioma = reader.GetString(reader.GetOrdinal("TC_Idioma"));
                            model.Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad"));
                            model.Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles"));
                            model.Id_Editorial = reader.GetInt32(reader.GetOrdinal("TN_Id_Editorial"));
                            model.Estado = reader.GetString(reader.GetOrdinal("TC_Estado"));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Material no encontrado.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                // Obtener autores asociados al material
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetAutoresByMaterial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.SelectedAutorIds.Add(reader.GetInt32(reader.GetOrdinal("TN_Id_Autor")));
                        }
                    }
                }

                LoadCategorias(model);
                LoadEditoriales(model);
                LoadAutores(model);
                LoadAnios();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el material: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Materiales/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Material model)
        {
            if (!ModelState.IsValid)
            {
                LoadCategorias(model);
                LoadEditoriales(model);
                LoadAutores(model);
                LoadAnios();
                return View(model);
            }

            try
            {
                using (var conn = new SqlConnection(_cs))
                {
                    conn.Open();

                    // 1. Actualizar el material
                    using (var cmd = new SqlCommand("usp_UpdateMaterial", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id_Material", model.Id_Material);
                        cmd.Parameters.AddWithValue("@Titulo", model.Titulo);
                        cmd.Parameters.AddWithValue("@ISBN", model.ISBN);
                        cmd.Parameters.AddWithValue("@Id_Categoria", model.Id_Categoria);
                        cmd.Parameters.AddWithValue("@FechaPublicacion", model.AnioPublicacion);
                        cmd.Parameters.AddWithValue("@Idioma", model.Idioma);
                        cmd.Parameters.AddWithValue("@Cantidad", model.Cantidad);
                        cmd.Parameters.AddWithValue("@Disponibles", model.Disponibles);
                        cmd.Parameters.AddWithValue("@Id_Editorial", model.Id_Editorial);
                        cmd.Parameters.AddWithValue("@Estado", model.Estado);

                        cmd.ExecuteNonQuery();
                    }

                    // 2. Eliminar autores anteriores
                    using (var cmd = new SqlCommand("usp_DeleteAutoresFromMaterial", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Id_Material", model.Id_Material);
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Agregar los nuevos autores seleccionados
                    foreach (var autorId in model.SelectedAutorIds)
                    {
                        using (var cmd = new SqlCommand("usp_AddAutorToMaterial", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@Id_Material", model.Id_Material);
                            cmd.Parameters.AddWithValue("@Id_Autor", autorId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                TempData["SuccessMessage"] = "Material actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex) when (
                   ex.Number == 55001
                || ex.Number == 55002
                || ex.Number == 55003)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ocurrió un error al actualizar el material: " + ex.Message;
            }

            LoadCategorias(model);
            LoadEditoriales(model);
            LoadAutores(model);
            LoadAnios();

            return View(model);
        }

        // GET: Materiales/AgregarEjemplares/5
        [HttpGet]
        public IActionResult AgregarEjemplares(int id)
        {
            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetMaterialById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var model = new Material
                            {
                                Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                                Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad")),
                                Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles"))
                            };
                            return View(model);
                        }
                    }
                }

                TempData["ErrorMessage"] = "Material no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el material: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Materiales/AgregarEjemplares
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarEjemplares(int id, int cantidadAgregar)
        {
            if (cantidadAgregar <= 0)
            {
                TempData["ErrorMessage"] = "La cantidad a agregar debe ser mayor a 0.";
                return RedirectToAction(nameof(AgregarEjemplares), new { id });
            }

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_AgregarEjemplaresMaterial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    cmd.Parameters.AddWithValue("@CantidadAgregar", cantidadAgregar);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = $"Se agregaron {cantidadAgregar} ejemplares correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al agregar ejemplares: " + ex.Message;
                return RedirectToAction(nameof(AgregarEjemplares), new { id });
            }
        }

        // GET: Materiales/Details/5
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            var model = new Material();

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetMaterialById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material"));
                            model.Titulo = reader.GetString(reader.GetOrdinal("TC_Titulo"));
                            model.ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN"));
                            model.Id_Categoria = reader.GetInt32(reader.GetOrdinal("TN_Id_Categoria"));
                            model.AnioPublicacion = reader.GetInt32(reader.GetOrdinal("TN_FechaPublicacion"));
                            model.Idioma = reader.GetString(reader.GetOrdinal("TC_Idioma"));
                            model.Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad"));
                            model.Disponibles = reader.GetInt32(reader.GetOrdinal("TN_Disponibles"));
                            model.Id_Editorial = reader.GetInt32(reader.GetOrdinal("TN_Id_Editorial"));
                            model.Estado = reader.GetString(reader.GetOrdinal("TC_Estado"));
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Material no encontrado.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }

                // Obtener nombres completos de categoría y editorial
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand(@"
                    SELECT
                        c.TC_Categoria AS CategoriaNombre,
                        e.TC_Editorial AS EditorialNombre
                    FROM TCTPB_Cat_Materiales m
                    INNER JOIN TCTPB_Cat_Categorias c ON m.TN_Id_Categoria = c.TN_Id_Categoria
                    INNER JOIN TCTPB_Cat_Editoriales e ON m.TN_Id_Editorial = e.TN_Id_Editoriales
                    WHERE m.TN_Id_Material = @Id_Material", conn))
                {
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.CategoriaNombre = reader.GetString(reader.GetOrdinal("CategoriaNombre"));
                            model.EditorialNombre = reader.GetString(reader.GetOrdinal("EditorialNombre"));
                        }
                    }
                }

                // Obtener autores del material
                var autores = new List<string>();
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetAutoresByMaterial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var nombre = reader.GetString(reader.GetOrdinal("TC_Nombre"));
                            var apellidos = reader.GetString(reader.GetOrdinal("TC_Apellidos"));
                            autores.Add($"{apellidos}, {nombre}");
                        }
                    }
                }
                model.Autores = string.Join("; ", autores);

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el material: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Materiales/DarDeBaja
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DarDeBaja(int id, int cantidad, string observacion)
        {
            if (string.IsNullOrWhiteSpace(observacion))
            {
                TempData["ErrorMessage"] = "Debe proporcionar una observación para dar de baja el material.";
                return RedirectToAction(nameof(Index));
            }

            if (cantidad <= 0)
            {
                TempData["ErrorMessage"] = "La cantidad a dar de baja debe ser mayor a 0.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Obtener el ID del usuario actual desde los claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    TempData["ErrorMessage"] = "No se pudo identificar el usuario actual.";
                    return RedirectToAction(nameof(Index));
                }

                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_DarDeBajaMaterial", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Material", id);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Id_Usuario", userId);
                    cmd.Parameters.AddWithValue("@Observacion", observacion);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = $"Se dieron de baja {cantidad} ejemplar(es) correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al dar de baja el material: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Materiales/HistorialBajas
        [HttpGet]
        public IActionResult HistorialBajas()
        {
            var bajas = new List<BajaMaterial>();

            try
            {
                using (var conn = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("usp_GetHistorialBajas", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bajas.Add(new BajaMaterial
                            {
                                Id_Baja = reader.GetInt32(reader.GetOrdinal("TN_Id_Baja")),
                                Id_Material = reader.GetInt32(reader.GetOrdinal("TN_Id_Material")),
                                TituloMaterial = reader.GetString(reader.GetOrdinal("TC_Titulo")),
                                ISBN = reader.GetString(reader.GetOrdinal("TC_ISBN")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("TN_Cantidad")),
                                Observacion = reader.GetString(reader.GetOrdinal("TC_Observacion")),
                                FechaBaja = reader.GetDateTime(reader.GetOrdinal("TF_Fecha_Baja")),
                                NombreUsuario = reader.GetString(reader.GetOrdinal("TC_UserName"))
                            });
                        }
                    }
                }

                return View(bajas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error al cargar el historial de bajas: " + ex.Message;
                return View(new List<BajaMaterial>());
            }
        }


    }//cierre class
}// cierre namespace