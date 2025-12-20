using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class BusquedaUsuariosViewModel
    {
        // Filtros de búsqueda
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public int? IdRol { get; set; }
        public bool? Activo { get; set; }

        // Resultados
        public List<UsuarioConsulta> Resultados { get; set; } = new List<UsuarioConsulta>();

        // Listas para dropdowns
        public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EstadosActivo { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Todos --" },
            new SelectListItem { Value = "true", Text = "Activos" },
            new SelectListItem { Value = "false", Text = "Inactivos" }
        };

        public bool HasResults => Resultados.Count > 0;
        public bool HasSearched { get; set; }
    }

    public class UsuarioConsulta
    {
        public int TN_Id_Usuario { get; set; }
        public string TC_UserName { get; set; } = string.Empty;
        public string TC_Email { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public int TN_Id_Rol { get; set; }
        public bool TB_Activo { get; set; }
        public int PrestamosActivos { get; set; }
        public int TotalPrestamos { get; set; }
    }
}