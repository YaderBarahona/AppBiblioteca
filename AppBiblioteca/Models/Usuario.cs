using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Usuario
    {
        public int Id_Usuario { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string UserName { get; set; }   // nombre a mostrar
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Cedula { get; set; }
       
        public string? Seccion { get; set; }
        public string? Telefono { get; set; }

        public bool Activo { get; set; }  // 1 = activo, 0 = dado de baja


        // para hacer el get de los usuarios con el sp
        public string Role { get; set; }


    }
}
