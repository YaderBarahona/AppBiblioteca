using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AppBiblioteca.Models
{
    public class Prestamo
    {
        // Campos de la tabla TCTPB_Pro_Prestamos
        public int TN_Id_Prestamo { get; set; }

        [Display(Name = "Usuario")]
        public int TN_Id_Usuario { get; set; }

        [Display(Name = "Estado")]
        public int TN_Id_Estado { get; set; }

        [Display(Name = "Fecha de Préstamo")]
        public DateTime? TF_FechaPrestamo { get; set; }

        [Display(Name = "Fecha de Devolución Esperada")]
        public DateTime? TF_FechaDevolucion { get; set; }

        [Display(Name = "Fecha de Devolución Real")]
        public DateTime? TF_FechaDevuelto { get; set; }

        [Display(Name = "Días de Atraso")]
        public int TN_DiasAtraso { get; set; }

        // Propiedades adicionales para mostrar información
        public string? NombreUsuario { get; set; }
        public string? EstadoNombre { get; set; }
        public string? EmailUsuario { get; set; }

        // Lista de materiales del préstamo
        public List<PrestamoDetalle> Detalles { get; set; } = new List<PrestamoDetalle>();

        // Propiedades calculadas
        public bool PuedeSerCancelado => TN_Id_Estado == 1; // Solo si está Reservado
        public bool PuedeSerAprobado => TN_Id_Estado == 1; // Solo si está Reservado
        public bool PuedeSerDevuelto => TN_Id_Estado == 2; // Solo si está Prestado
        public bool TieneAtraso => TN_DiasAtraso > 0;
    }

    public class PrestamoDetalle
    {
        // Campos de la tabla TCTPB_Reg_PrestamoDetalle
        public int TN_Id_Detalle { get; set; }
        public int TN_Id_Prestamo { get; set; }
        public int TN_Id_Material { get; set; }

        // Propiedades adicionales para mostrar
        public string? TituloMaterial { get; set; }
        public string? ISBN { get; set; }
        public string? Autores { get; set; }
    }

    // ViewModel para crear nueva reserva
    public class CrearReservaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar al menos un material")]
        [Display(Name = "Materiales a Reservar")]
        public List<int> MaterialesSeleccionados { get; set; } = new List<int>();

        // Lista de materiales disponibles
        public List<MaterialDisponible> MaterialesDisponibles { get; set; } = new List<MaterialDisponible>();
    }

    public class MaterialDisponible
    {
        public int TN_Id_Material { get; set; }
        public string? TC_Titulo { get; set; }
        public string? TC_ISBN { get; set; }
        public int TN_Disponibles { get; set; }
        public string? Autores { get; set; }
        public string? Categoria { get; set; }
        public bool Seleccionado { get; set; }
    }
}