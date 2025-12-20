namespace AppBiblioteca.Models
{
    public class EstadisticasRecursosViewModel
    {
        public int TotalRecursos { get; set; }
        public List<AsignaturaEstadistica> PorAsignatura { get; set; } = new List<AsignaturaEstadistica>();
        public List<ProfesorEstadistica> PorProfesor { get; set; } = new List<ProfesorEstadistica>();
        public List<TipoEstadistica> PorTipo { get; set; } = new List<TipoEstadistica>();
    }

    public class AsignaturaEstadistica
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class ProfesorEstadistica
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class TipoEstadistica
    {
        public string Tipo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}