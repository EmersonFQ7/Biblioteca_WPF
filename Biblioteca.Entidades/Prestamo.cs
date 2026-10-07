using System;
using System.Collections.Generic;

namespace Biblioteca.Entidades
{
    public class Prestamo
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public DateTime FechaPrestamo { get; set; } = DateTime.Now;
        public DateTime FechaLimite { get; set; }
        public string Estado { get; set; } = "Pendiente";

        // Propiedades auxiliares para WPF (vienen de JOINs)
        public string NombreSocio { get; set; } = string.Empty;
        public string DniSocio { get; set; } = string.Empty;

        // Colección de detalles del préstamo
        public List<DetallePrestamo> Detalles { get; set; } = new List<DetallePrestamo>();
    }
}