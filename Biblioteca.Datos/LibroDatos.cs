using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class LibroDatos
    {
        public async Task<List<Libro>> ListarAsync(string filtro = "")
        {
            var lista = new List<Libro>();

            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId,
                                      l.Ejemplares, l.Activo, a.Nombre AS NombreAutor
                               FROM Libros l
                               INNER JOIN Autores a ON a.AutorId = l.AutorId
                               WHERE l.Activo = 1
                                 AND (@filtro = ''
                                      OR l.Titulo LIKE '%' + @filtro + '%'
                                      OR a.Nombre LIKE '%' + @filtro + '%')
                               ORDER BY l.Titulo";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@filtro", SqlDbType.VarChar, 100).Value = filtro ?? "";

                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Libro
                            {
                                Id = dr.GetInt32(0),
                                Titulo = dr.GetString(1),
                                ISBN = dr.GetString(2),
                                AutorId = dr.GetInt32(3),
                                Ejemplares = dr.GetInt32(4),
                                Activo = dr.GetBoolean(5),
                                NombreAutor = dr.GetString(6)
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public async Task<Libro?> ObtenerPorIdAsync(int libroId)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT l.LibroId, l.Titulo, l.ISBN, l.AutorId,
                                      l.Ejemplares, l.Activo, a.Nombre
                               FROM Libros l
                               INNER JOIN Autores a ON a.AutorId = l.AutorId
                               WHERE l.LibroId = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = libroId;

                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                        {
                            return new Libro
                            {
                                Id = dr.GetInt32(0),
                                Titulo = dr.GetString(1),
                                ISBN = dr.GetString(2),
                                AutorId = dr.GetInt32(3),
                                Ejemplares = dr.GetInt32(4),
                                Activo = dr.GetBoolean(5),
                                NombreAutor = dr.GetString(6)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> ExisteISBNAsync(string isbn, int libroIdExcluir = 0)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT COUNT(1) FROM Libros
                               WHERE ISBN = @isbn AND LibroId <> @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@isbn", SqlDbType.VarChar, 20).Value = isbn;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = libroIdExcluir;

                    await cn.OpenAsync();
                    int total = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    return total > 0;
                }
            }
        }

        public async Task<int> InsertarAsync(Libro libro)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares, Activo)
                               VALUES (@titulo, @isbn, @autorId, @ejemplares, 1);
                               SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@titulo", SqlDbType.VarChar, 150).Value = libro.Titulo;
                    cmd.Parameters.Add("@isbn", SqlDbType.VarChar, 20).Value = libro.ISBN;
                    cmd.Parameters.Add("@autorId", SqlDbType.Int).Value = libro.AutorId;
                    cmd.Parameters.Add("@ejemplares", SqlDbType.Int).Value = libro.Ejemplares;

                    await cn.OpenAsync();
                    return (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }
            }
        }

        public async Task<int> ActualizarAsync(Libro libro)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"UPDATE Libros
                               SET Titulo = @titulo, ISBN = @isbn,
                                   AutorId = @autorId, Ejemplares = @ejemplares
                               WHERE LibroId = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@titulo", SqlDbType.VarChar, 150).Value = libro.Titulo;
                    cmd.Parameters.Add("@isbn", SqlDbType.VarChar, 20).Value = libro.ISBN;
                    cmd.Parameters.Add("@autorId", SqlDbType.Int).Value = libro.AutorId;
                    cmd.Parameters.Add("@ejemplares", SqlDbType.Int).Value = libro.Ejemplares;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = libro.Id;

                    await cn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> EliminarLogicoAsync(int libroId)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = "UPDATE Libros SET Activo = 0 WHERE LibroId = @id";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = libroId;

                    await cn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> TienePrestamosPendientesAsync(int libroId)
        {
            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT COUNT(1)
                               FROM DetallePrestamo
                               WHERE LibroId = @id AND FechaDevolucion IS NULL";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = libroId;

                    await cn.OpenAsync();
                    int total = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    return total > 0;
                }
            }
        }
    }
}