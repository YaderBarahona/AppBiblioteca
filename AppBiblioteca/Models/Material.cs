using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class Material
    {
        public int Id_Material { get; set; }

        [Required, StringLength(200)]
        public string ? Titulo { get; set; }

        [Required, StringLength(50)] 
        public string? ISBN { get; set; } 

        [Required]
        [Display(Name = "Categoría")]
        public int Id_Categoria { get; set; }

        /*[Required]
        [DataType(DataType.Date)]
        [Display(Name = "Fecha de publicación")]
        public DateTime FechaPublicacion { get; set; }*/


        [Required(ErrorMessage = "Debe seleccionar un año")]
        [Display(Name = "Año de publicación")]
        public int AnioPublicacion { get; set; }




        [Required, StringLength(50)]
        public string ? Idioma { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Cantidad { get; set; }

        // Pretados se inicializa a 0 al crear, no va en el formulario
        public int Pretados { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Disponibles { get; set; }

        [Required]
        [Display(Name = "Editorial")]
        public int Id_Editorial { get; set; }

        [Required, StringLength(50)]
        public string ? Estado { get; set; }


        public string ? Autores { get; set; }

        // para agregar a la tabla MaterialesAutres de la Base de Datos
        [Display(Name = "Autores")]
        [Required(ErrorMessage = "Selecciona al menos un autor.")]
        public List<int> SelectedAutorIds { get; set; } = new();

        // Para poblar el dropdown de autores
        public IEnumerable<SelectListItem> CategoryList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> EditorialList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> AutorList { get; set; } = new List<SelectListItem>();

        // Para desplegar nombres en dropdowns o listados
        public string ? CategoriaNombre { get; set; }
        public string ? EditorialNombre { get; set; }
    }

}
