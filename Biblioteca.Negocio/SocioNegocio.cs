using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class SocioNegocio
    {
        private readonly SocioDatos _datos = new SocioDatos();

        public Task<List<Socio>> ListarAsync(string filtro = "") => _datos.ListarAsync(filtro);

        public async Task<int> InsertarAsync(Socio socio)
        {
            ValidarCampos(socio);

            if (await _datos.ExisteDNIAsync(socio.DNI))
                throw new ReglaNegocioException($"Ya existe un socio con el DNI '{socio.DNI}'.");

            return await _datos.InsertarAsync(socio);
        }

        public async Task<int> ActualizarAsync(Socio socio)
        {
            if (socio.Id <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio para actualizar.");

            ValidarCampos(socio);

            if (await _datos.ExisteDNIAsync(socio.DNI, socio.Id))
                throw new ReglaNegocioException($"Ya existe otro socio con el DNI '{socio.DNI}'.");

            return await _datos.ActualizarAsync(socio);
        }

        public async Task<int> EliminarAsync(int socioId)
        {
            if (socioId <= 0)
                throw new ReglaNegocioException("Debe seleccionar un socio para eliminar.");

            int pendientes = await _datos.ContarLibrosPendientesAsync(socioId);
            if (pendientes > 0)
                throw new ReglaNegocioException(
                    $"No se puede eliminar el socio: tiene {pendientes} libro(s) pendiente(s) de devolucion.");

            return await _datos.EliminarLogicoAsync(socioId);
        }

        private static void ValidarCampos(Socio socio)
        {
            if (string.IsNullOrWhiteSpace(socio.DNI))
                throw new ReglaNegocioException("El DNI es obligatorio.");

            if (socio.DNI.Length != 8 || !long.TryParse(socio.DNI, out _))
                throw new ReglaNegocioException("El DNI debe tener 8 digitos numericos.");

            if (string.IsNullOrWhiteSpace(socio.Nombre))
                throw new ReglaNegocioException("El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(socio.Email) || !socio.Email.Contains("@"))
                throw new ReglaNegocioException("Debe ingresar un email valido.");
        }
    }
}