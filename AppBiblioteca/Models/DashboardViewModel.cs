using System;
using System.Collections.Generic;

namespace AppBiblioteca.Models
{
    public class DashboardViewModel
    {
        // Estadísticas Generales
        public int TotalMateriales { get; set; }
        public int TotalEjemplares { get; set; }
        public int EjemplaresDisponibles { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalEstudiantes { get; set; }
        public int PrestamosActivos { get; set; }
        public int ReservasPendientes { get; set; }
        public int PrestamosAtrasados { get; set; }
        public int TotalRecursosExternos { get; set; }

        // Datos para gráficos
        public List<PrestamosPorEstado> PrestamosPorEstado { get; set; } = new List<PrestamosPorEstado>();
        public List<PrestamosPorMes> PrestamosPorMes { get; set; } = new List<PrestamosPorMes>();
        public List<PrestamosPorCategoria> PrestamosPorCategoria { get; set; } = new List<PrestamosPorCategoria>();
        public List<MaterialTop> Top10Materiales { get; set; } = new List<MaterialTop>();
        public List<UsuarioTop> Top10Usuarios { get; set; } = new List<UsuarioTop>();

        // Propiedades calculadas
        public decimal PorcentajeDisponibilidad => TotalEjemplares > 0
            ? Math.Round((EjemplaresDisponibles * 100.0m / TotalEjemplares), 2)
            : 0;

        public decimal TasaPrestamos => TotalEjemplares > 0
            ? Math.Round(((TotalEjemplares - EjemplaresDisponibles) * 100.0m / TotalEjemplares), 2)
            : 0;

        public int EjemplaresPrestados => TotalEjemplares - EjemplaresDisponibles;
    }

    public class PrestamosPorEstado
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class PrestamosPorMes
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string NombreMes { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
        public int Devueltos { get; set; }
        public int Atrasados { get; set; }
    }

    public class PrestamosPorCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadPrestamos { get; set; }
    }

    public class MaterialTop
    {
        public int TN_Id_Material { get; set; }
        public string TC_Titulo { get; set; } = string.Empty;
        public string TC_ISBN { get; set; } = string.Empty;
        public string TC_Categoria { get; set; } = string.Empty;
        public int VecesPrestado { get; set; }
        public int TN_Disponibles { get; set; }
        public int TN_Cantidad { get; set; }
    }

    public class UsuarioTop
    {
        public int TN_Id_Usuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string TC_Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public int TotalPrestamos { get; set; }
        public int PrestamosActivos { get; set; }
        public int PrestamosAtrasados { get; set; }
    }
}