using System;

namespace Biblioteca.Entidades
{
    public class DetallePrestamo
    {
        public int PrestamoId { get; set; }
        public int LibroId { get; set; }
        public DateTime? FechaDevolucion { get; set; }  // nullable, puede ser NULL

        // Propiedad auxiliar para WPF
        public string TituloLibro { get; set; } = string.Empty;
    }
}