using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    // ViewModel para Reporte de Préstamos por Período
    public class ReportePrestamosViewModel
    {
        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        public DateTime? FechaFin { get; set; }

        public List<PrestamoPeriodo> Prestamos { get; set; } = new List<PrestamoPeriodo>();
        public int TotalPrestamos => Prestamos.Count;
    }

    public class PrestamoPeriodo
    {
        public int TN_Id_Prestamo { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string TC_Email { get; set; } = string.Empty;
        public string EstadoPrestamo { get; set; } = string.Empty;
        public DateTime? TF_FechaPrestamo { get; set; }
        public DateTime? TF_FechaDevolucion { get; set; }
        public DateTime? TF_FechaDevuelto { get; set; }
        public int TN_DiasAtraso { get; set; }
        public int CantidadMateriales { get; set; }
    }

    // ViewModel para Reporte de Atrasos
    public class ReporteAtrasosViewModel
    {
        public List<PrestamoAtrasado> PrestamosAtrasados { get; set; } = new List<PrestamoAtrasado>();
        public int TotalAtrasados => PrestamosAtrasados.Count;
    }

    public class PrestamoAtrasado
    {
        public int TN_Id_Prestamo { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string TC_Email { get; set; } = string.Empty;
        public string? TC_Telefono { get; set; }
        public DateTime? TF_FechaPrestamo { get; set; }
        public DateTime? TF_FechaDevolucion { get; set; }
        public int DiasAtraso { get; set; }
        public int CantidadMateriales { get; set; }
    }

    // ViewModel para Reporte de Disponibilidad
    public class ReporteDisponibilidadViewModel
    {
        public List<DisponibilidadCategoria> Categorias { get; set; } = new List<DisponibilidadCategoria>();
    }

    public class DisponibilidadCategoria
    {
        public string Categoria { get; set; } = string.Empty;
        public int TotalMateriales { get; set; }
        public int TotalEjemplares { get; set; }
        public int EjemplaresDisponibles { get; set; }
        public int EjemplaresPrestados { get; set; }
        public decimal PorcentajeDisponibilidad { get; set; }
    }

    // ViewModel para Reporte de Recursos Externos
    public class ReporteRecursosExternosViewModel
    {
        public List<RecursoExternoPorAsignatura> Recursos { get; set; } = new List<RecursoExternoPorAsignatura>();
    }

    public class RecursoExternoPorAsignatura
    {
        public string Asignatura { get; set; } = string.Empty;
        public string? TipoContenido { get; set; }
        public int CantidadRecursos { get; set; }
        public DateTime? UltimaPublicacion { get; set; }
    }
}