using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class HistorialPrestamosViewModel
    {
        // Filtros de búsqueda
        public int? IdUsuario { get; set; }
        public int? IdEstado { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? ConAtraso { get; set; }

        // Resultados
        public List<PrestamoHistorial> Resultados { get; set; } = new List<PrestamoHistorial>();

        // Listas para dropdowns
        public IEnumerable<SelectListItem> Usuarios { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Estados { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> FiltroAtraso { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Todos --" },
            new SelectListItem { Value = "true", Text = "Solo con atraso" },
            new SelectListItem { Value = "false", Text = "Sin atraso" }
        };

        public bool HasResults => Resultados.Count > 0;
        public bool HasSearched { get; set; }
    }

    public class PrestamoHistorial
    {
        public int TN_Id_Prestamo { get; set; }
        public int TN_Id_Usuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string EmailUsuario { get; set; } = string.Empty;
        public int TN_Id_Estado { get; set; }
        public string EstadoNombre { get; set; } = string.Empty;
        public DateTime? TF_FechaPrestamo { get; set; }
        public DateTime? TF_FechaDevolucion { get; set; }
        public DateTime? TF_FechaDevuelto { get; set; }
        public int TN_DiasAtraso { get; set; }
        public int CantidadMateriales { get; set; }
        public List<PrestamoDetalle> Detalles { get; set; } = new List<PrestamoDetalle>();
    }

    public class EstadisticasViewModel
    {
        // Estadísticas de Materiales
        public int TotalMateriales { get; set; }
        public int TotalEjemplares { get; set; }
        public int TotalDisponibles { get; set; }
        public int TotalPrestados { get; set; }
        public int MaterialesDisponibles { get; set; }
        public int MaterialesDadosBaja { get; set; }

        // Estadísticas de Préstamos
        public int TotalPrestamos { get; set; }
        public int Reservados { get; set; }
        public int Prestados { get; set; }
        public int Devueltos { get; set; }
        public int Cancelados { get; set; }
        public int ConAtraso { get; set; }

        // Top materiales y usuarios
        public List<MaterialMasPrestado> MaterialesMasPrestados { get; set; } = new List<MaterialMasPrestado>();
        public List<UsuarioMasActivo> UsuariosMasActivos { get; set; } = new List<UsuarioMasActivo>();
    }

    public class MaterialMasPrestado
    {
        public int TN_Id_Material { get; set; }
        public string TC_Titulo { get; set; } = string.Empty;
        public string TC_ISBN { get; set; } = string.Empty;
        public string TC_Categoria { get; set; } = string.Empty;
        public int VecesPrestado { get; set; }
        public int TN_Disponibles { get; set; }
        public int TN_Prestados { get; set; }
    }

    public class UsuarioMasActivo
    {
        public int TN_Id_Usuario { get; set; }
        public string TC_UserName { get; set; } = string.Empty;
        public string TC_Email { get; set; } = string.Empty;
        public string TC_Role { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
        public int PrestamosActivos { get; set; }
        public int PrestamosAtrasados { get; set; }
    }
}