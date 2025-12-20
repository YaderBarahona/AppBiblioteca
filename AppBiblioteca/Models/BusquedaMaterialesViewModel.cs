using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class BusquedaMaterialesViewModel
    {
        // Filtros de búsqueda
        public string? Titulo { get; set; }
        public string? ISBN { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdEditorial { get; set; }
        public int? IdAutor { get; set; }
        public string? Estado { get; set; }
        public int? AnioDesde { get; set; }
        public int? AnioHasta { get; set; }

        // Resultados
        public List<Material> Resultados { get; set; } = new List<Material>();

        // Listas para dropdowns
        public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Editoriales { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Autores { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Estados { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "-- Todos --" },
            new SelectListItem { Value = "Disponible", Text = "Disponible" },
            new SelectListItem { Value = "No Disponible", Text = "No Disponible" },
            new SelectListItem { Value = "Dado de Baja", Text = "Dado de Baja" }
        };

        public bool HasResults => Resultados.Count > 0;
        public bool HasSearched { get; set; }
    }
}