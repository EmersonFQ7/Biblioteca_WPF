using System;

namespace Biblioteca.Negocio
{
    // Excepcion para violaciones de reglas de negocio.
    // La capa WPF captura SOLO esta excepcion, nunca SqlException.
    public class ReglaNegocioException : Exception
    {
        public ReglaNegocioException(string mensaje) : base(mensaje) { }
    }
}