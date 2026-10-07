using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class LibroNegocio
    {
        private readonly LibroDatos _datos = new LibroDatos();

        public Task<List<Libro>> ListarAsync(string filtro = "") => _datos.ListarAsync(filtro);

        public async Task<int> InsertarAsync(Libro libro)
        {
            ValidarCampos(libro);

            if (await _datos.ExisteISBNAsync(libro.ISBN))
                throw new ReglaNegocioException($"Ya existe un libro con el ISBN '{libro.ISBN}'.");

            return await _datos.InsertarAsync(libro);
        }

        public async Task<int> ActualizarAsync(Libro libro)
        {
            if (libro.Id <= 0)
                throw new ReglaNegocioException("Debe seleccionar un libro para actualizar.");

            ValidarCampos(libro);

            if (await _datos.ExisteISBNAsync(libro.ISBN, libro.Id))
                throw new ReglaNegocioException($"Ya existe otro libro con el ISBN '{libro.ISBN}'.");

            return await _datos.ActualizarAsync(libro);
        }

        public async Task<int> EliminarAsync(int libroId)
        {
            if (libroId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un libro para eliminar.");

            if (await _datos.TienePrestamosPendientesAsync(libroId))
                throw new ReglaNegocioException(
                    "No se puede eliminar el libro: tiene prestamos pendientes.");

            return await _datos.EliminarLogicoAsync(libroId);
        }

        private static void ValidarCampos(Libro libro)
        {
            if (string.IsNullOrWhiteSpace(libro.Titulo))
                throw new ReglaNegocioException("El titulo es obligatorio.");

            if (string.IsNullOrWhiteSpace(libro.ISBN))
                throw new ReglaNegocioException("El ISBN es obligatorio.");

            if (libro.AutorId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un autor.");

            if (libro.Ejemplares < 0)
                throw new ReglaNegocioException("Los ejemplares no pueden ser negativos.");
        }
    }
}