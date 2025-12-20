using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Categoria
    {

        public int Id_Categoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Categoría")]
        public string CategoriaNombre { get; set; }

        [Required(ErrorMessage = "Agrega una descripción")]
        [StringLength(150)]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }
    }

}
