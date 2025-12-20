using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppBiblioteca.Models
{
    public class ContenidosExternosViewModel
    {
        // Filtros
        public int? FiltroAsignatura { get; set; }
        public int? FiltroTipoContenido { get; set; }
        public bool MostrarInactivos { get; set; } = false;

        // Resultados
        public List<ContenidoExterno> Contenidos { get; set; } = new List<ContenidoExterno>();

        // Listas para filtros
        public IEnumerable<SelectListItem> AsignaturasList { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> TiposContenidoList { get; set; } = new List<SelectListItem>();

        // Propiedades de ayuda
        public bool HasResults => Contenidos.Count > 0;
        public int TotalResultados => Contenidos.Count;
    }
}