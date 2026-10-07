using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class PrestamoNegocio
    {
        private const int MAXIMO_LIBROS_PENDIENTES = 3;
        private const decimal MULTA_POR_DIA = 1.50m;

        private readonly PrestamoDatos _datos = new PrestamoDatos();
        private readonly LibroDatos _libros = new LibroDatos();
        private readonly SocioDatos _socios = new SocioDatos();

        public Task<List<Prestamo>> ListarPendientesPorSocioAsync(int socioId)
            => _datos.ListarPendientesPorSocioAsync(socioId);

        public async Task<List<Prestamo>> ReportePorFechasAsync(DateTime desde, DateTime hasta)
        {
            if (desde > hasta)
                throw new ReglaNegocioException("La fecha 'desde' no puede ser mayor que 'hasta'.");

            return await _datos.ReportePorFechasAsync(desde, hasta);
        }

        public async Task<int> RegistrarAsync(Prestamo prestamo)
        {
            // Validaciones basicas
            if (prestamo.SocioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio.");

            if (prestamo.Detalles == null || prestamo.Detalles.Count == 0)
                throw new ReglaNegocioException("Debe agregar al menos un libro al prestamo.");

            if (prestamo.FechaLimite <= prestamo.FechaPrestamo)
                throw new ReglaNegocioException("La fecha limite debe ser posterior a la fecha de prestamo.");

            // Regla: cuantos pendientes tiene el socio actualmente
            int pendientesActuales = await _socios.ContarLibrosPendientesAsync(prestamo.SocioId);
            int totalFinal = pendientesActuales + prestamo.Detalles.Count;

            if (totalFinal > MAXIMO_LIBROS_PENDIENTES)
                throw new ReglaNegocioException(
                    $"El socio ya tiene {pendientesActuales} libro(s) pendiente(s). " +
                    $"No puede exceder el maximo de {MAXIMO_LIBROS_PENDIENTES}.");

            // Regla: cada libro debe estar activo y con ejemplares
            foreach (var det in prestamo.Detalles)
            {
                var libro = await _libros.ObtenerPorIdAsync(det.LibroId);

                if (libro == null)
                    throw new ReglaNegocioException($"El libro con Id {det.LibroId} no existe.");

                if (!libro.Activo)
                    throw new ReglaNegocioException($"El libro '{libro.Titulo}' no esta activo.");

                if (libro.Ejemplares <= 0)
                    throw new ReglaNegocioException($"El libro '{libro.Titulo}' no tiene ejemplares disponibles.");
            }

            // Si todo esta bien -> llamamos a Datos (transaccion)
            return await _datos.RegistrarAsync(prestamo);
        }

        public async Task<decimal> RegistrarDevolucionAsync(
            int prestamoId, int libroId, DateTime fechaDevolucion, DateTime fechaLimite)
        {
            if (prestamoId <= 0 || libroId <= 0)
                throw new ReglaNegocioException("Prestamo o libro invalido.");

            await _datos.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion);

            // Calcular multa (en la capa Negocio, no en BD)
            return CalcularMulta(fechaLimite, fechaDevolucion);
        }

        public decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion)
        {
            if (fechaDevolucion.Date <= fechaLimite.Date)
                return 0m;

            int diasRetraso = (fechaDevolucion.Date - fechaLimite.Date).Days;
            return diasRetraso * MULTA_POR_DIA;
        }
    }
}