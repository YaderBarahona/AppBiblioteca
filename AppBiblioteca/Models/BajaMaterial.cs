using System;
using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class BajaMaterial
    {
        public int Id_Baja { get; set; }
        public int Id_Material { get; set; }
        public string TituloMaterial { get; set; }
        public string ISBN { get; set; }
        public int Cantidad { get; set; }
        public string Observacion { get; set; }
        public DateTime FechaBaja { get; set; }
        public string NombreUsuario { get; set; }
    }
}
