using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class SocioDatos
    {
        public async Task<List<Socio>> ListarAsync(string filtro = "")
        {
            var lista = new List<Socio>();

            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT SocioId, DNI, Nombre, Email, Activo
                               FROM Socios
                               WHERE Activo = 1
                                 AND (@filtro = ''
                                      OR Nombre LIKE '%' + @filtro + '%'
                                      OR DNI LIKE '%' + @filtro + '%')
                               ORDER BY Nombre";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@filtro", SqlDbType.VarChar, 100).Value = filtro ?? "";

                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Socio
                            {
                                Id = dr.GetInt32(0),
                                DNI = dr.GetString(1),
                                Nombre = dr.GetString(2),
                                Email = dr.GetString(3),
                                Activo = dr.GetBoolean(4)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<bool> ExisteDNIAsync(string dni, int socioIdExcluir = 0)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT COUNT(1) FROM Socios
                               WHERE DNI = @dni AND SocioId <> @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@dni", SqlDbType.VarChar, 8).Value = dni;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = socioIdExcluir;

                    await cn.OpenAsync();
                    int total = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    return total > 0;
                }
            }
        }

        public async Task<int> InsertarAsync(Socio socio)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"INSERT INTO Socios (DNI, Nombre, Email, Activo)
                               VALUES (@dni, @nombre, @email, 1);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@dni", SqlDbType.VarChar, 8).Value = socio.DNI;
                    cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = socio.Nombre;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = socio.Email;

                    await cn.OpenAsync();
                    return (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }
            }
        }

        public async Task<int> ActualizarAsync(Socio socio)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"UPDATE Socios
                               SET DNI = @dni, Nombre = @nombre, Email = @email
                               WHERE SocioId = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@dni", SqlDbType.VarChar, 8).Value = socio.DNI;
                    cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 100).Value = socio.Nombre;
                    cmd.Parameters.Add("@email", SqlDbType.VarChar, 100).Value = socio.Email;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = socio.Id;

                    await cn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> EliminarLogicoAsync(int socioId)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = "UPDATE Socios SET Activo = 0 WHERE SocioId = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = socioId;

                    await cn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> ContarLibrosPendientesAsync(int socioId)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT COUNT(1)
                               FROM Prestamos p
                               INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                               WHERE p.SocioId = @id AND d.FechaDevolucion IS NULL";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = socioId;

                    await cn.OpenAsync();
                    return (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }
            }
        }
    }
}