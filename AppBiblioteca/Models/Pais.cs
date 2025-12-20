using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Pais
    {
        public int Id_Pais { get; set; }

        [Required(ErrorMessage = "Debe completar el campo.")]
        [StringLength(100)]
        public string NombrePais { get; set; }

    }
}
