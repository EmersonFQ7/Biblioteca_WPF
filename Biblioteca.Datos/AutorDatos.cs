using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class AutorDatos
    {
        public async Task<List<Autor>> ListarActivosAsync()
        {
            var lista = new List<Autor>();

            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT AutorId, Nombre, Nacionalidad, Activo
                               FROM Autores
                               WHERE Activo = 1
                               ORDER BY Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Autor
                            {
                                Id = dr.GetInt32(0),
                                Nombre = dr.GetString(1),
                                Nacionalidad = dr.GetString(2),
                                Activo = dr.GetBoolean(3)
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}