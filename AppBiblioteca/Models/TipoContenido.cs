using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class TipoContenido
    {
        public int TN_Id_TipoContenido { get; set; }

        [Required(ErrorMessage = "El tipo de contenido es requerido")]
        [StringLength(100, ErrorMessage = "El tipo no puede exceder 100 caracteres")]
        [Display(Name = "Tipo de Contenido")]
        public string TC_TipoContenido { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El icono no puede exceder 50 caracteres")]
        [Display(Name = "Icono Bootstrap")]
        public string? TC_Icono { get; set; }
    }
}