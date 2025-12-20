using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Editorial
    {
        public int Id_Editorial { get; set; }

        // [Required, StringLength(150)]
        [Required(ErrorMessage = "Debe ingresar el nombre del editorial")]

        public string EditorialNombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un país.")]
        public int? Id_Pais { get; set; }

        // Para mostrar el nombre del país en listados
        public string NombrePais { get; set; }
    }

}
