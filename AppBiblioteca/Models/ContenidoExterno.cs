using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class ContenidoExterno
    {
        public int TN_Id_Contenido { get; set; }

        [Required(ErrorMessage = "El título es requerido")]
        [StringLength(300, ErrorMessage = "El título no puede exceder 300 caracteres")]
        [Display(Name = "Título")]
        public string TC_Titulo { get; set; } = string.Empty;

        [Display(Name = "Usuario")]
        public int TN_Id_Usuario { get; set; }

        [Display(Name = "Nombre del Profesor")]
        public string NombreProfesor { get; set; } = string.Empty;

        [Required(ErrorMessage = "La asignatura es requerida")]
        [Display(Name = "Asignatura")]
        public int TN_Id_Asignatura { get; set; }

        [Display(Name = "Nombre de Asignatura")]
        public string NombreAsignatura { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de contenido es requerido")]
        [Display(Name = "Tipo de Contenido")]
        public int TN_Id_TipoContenido { get; set; }

        [Display(Name = "Tipo")]
        public string TC_TipoContenido { get; set; } = string.Empty;

        public string? TC_Icono { get; set; }

        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        [Display(Name = "Descripción")]
        public string? TC_Descripcion { get; set; }

        [Required(ErrorMessage = "La URL es requerida")]
        [StringLength(500, ErrorMessage = "La URL no puede exceder 500 caracteres")]
        [Url(ErrorMessage = "Debe ser una URL válida")]
        [Display(Name = "URL del Recurso")]
        public string TC_URL { get; set; } = string.Empty;

        [Display(Name = "Fecha de Creación")]
        public DateTime TF_FechaCreacion { get; set; }

        [Display(Name = "Fecha de Modificación")]
        public DateTime? TF_FechaModificacion { get; set; }

        [Display(Name = "Activo")]
        public bool TB_Activo { get; set; } = true;

        // Propiedades para los dropdowns
        public IEnumerable<SelectListItem> AsignaturasList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TiposContenidoList { get; set; } = new List<SelectListItem>();
    }
}