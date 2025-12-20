namespace AppBiblioteca.Models
{
    public class Libro
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Autor { get; set; }
        public string ISBN { get; set; }
        public string Categoria { get; set; }
        public string Editorial { get; set; }
        public string Idioma { get; set; }
        public int CantidadTotal { get; set; }
        public int CantidadDisponible { get; set; }

    }
}
