using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Asignatura
    {
        public int TN_Id_Asignatura { get; set; }

        [Required(ErrorMessage = "El nombre de la asignatura es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        [Display(Name = "Nombre")]
        public string TC_Nombre { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El código no puede exceder 50 caracteres")]
        [Display(Name = "Código")]
        public string? TC_Codigo { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
        [Display(Name = "Descripción")]
        public string? TC_Descripcion { get; set; }

        [Display(Name = "Activo")]
        public bool TB_Activo { get; set; } = true;
    }
}