using System.Collections.Generic;
using System.Threading.Tasks;
using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio
{
    public class AutorNegocio
    {
        private readonly AutorDatos _datos = new AutorDatos();

        public Task<List<Autor>> ListarActivosAsync() => _datos.ListarActivosAsync();
    }
}