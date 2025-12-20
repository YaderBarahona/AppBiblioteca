using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Autor
    {
        public int Id_Autor { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; }

        [Required, StringLength(200)]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un país.")]
        public int? Id_Pais { get; set; }

        public string NombrePais { get; set; }  // Para el JOIN con Paises
    }

}
