using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;

namespace Biblioteca.Datos
{
    internal static class Conexion
    {
        public static SqlConnection Obtener()
        {
            string cadena = ConfigurationManager
                .ConnectionStrings["BibliotecaDB"]
                .ConnectionString;

            return new SqlConnection(cadena);
        }
    }
}