using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class BusquedaRecursosViewModel
    {
        // Criterios de búsqueda
        [Display(Name = "Buscar en título y descripción")]
        public string? TextoBusqueda { get; set; }

        [Display(Name = "Asignatura")]
        public int? FiltroAsignatura { get; set; }

        [Display(Name = "Nombre del Profesor")]
        public string? NombreProfesor { get; set; }

        [Display(Name = "Tipo de Contenido")]
        public int? FiltroTipoContenido { get; set; }

        [Display(Name = "Desde")]
        [DataType(DataType.Date)]
        public DateTime? FechaDesde { get; set; }

        [Display(Name = "Hasta")]
        [DataType(DataType.Date)]
        public DateTime? FechaHasta { get; set; }

        // Resultados
        public List<RecursoExterno> Resultados { get; set; } = new List<RecursoExterno>();

        // Listas para filtros
        public IEnumerable<SelectListItem> AsignaturasList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TiposContenidoList { get; set; } = new List<SelectListItem>();

        // Propiedades de ayuda
        public bool HasResults => Resultados.Count > 0;
        public int TotalResultados => Resultados.Count;
        public bool HasSearched { get; set; } = false;
    }

    public class RecursoExterno
    {
        public int TN_Id_Contenido { get; set; }
        public string TC_Titulo { get; set; } = string.Empty;
        public int TN_Id_Usuario { get; set; }
        public string NombreProfesor { get; set; } = string.Empty;
        public string? EmailProfesor { get; set; }
        public int TN_Id_Asignatura { get; set; }
        public string NombreAsignatura { get; set; } = string.Empty;
        public string? CodigoAsignatura { get; set; }
        public int TN_Id_TipoContenido { get; set; }
        public string TC_TipoContenido { get; set; } = string.Empty;
        public string? TC_Icono { get; set; }
        public string? TC_Descripcion { get; set; }
        public string TC_URL { get; set; } = string.Empty;
        public DateTime TF_FechaCreacion { get; set; }
        public DateTime? TF_FechaModificacion { get; set; }
    }
}