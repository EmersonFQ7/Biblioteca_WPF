using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Biblioteca.Entidades;

namespace Biblioteca.Datos
{
    public class PrestamoDatos
    {
        // Registra prestamo + detalles + descuenta ejemplares en una transaccion
        public async Task<int> RegistrarAsync(Prestamo prestamo)
        {
            using (var cn = Conexion.Obtener())
            {
                await cn.OpenAsync();

                using (var tx = (SqlTransaction)await cn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1) Insertar cabecera
                        string sqlCab = @"INSERT INTO Prestamos
                                              (SocioId, FechaPrestamo, FechaLimite, Estado)
                                          VALUES (@socio, @fecha, @limite, 'Pendiente');
                                          SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        int prestamoId;
                        using (var cmd = new SqlCommand(sqlCab, cn, tx))
                        {
                            cmd.Parameters.Add("@socio", SqlDbType.Int).Value = prestamo.SocioId;
                            cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = prestamo.FechaPrestamo;
                            cmd.Parameters.Add("@limite", SqlDbType.DateTime).Value = prestamo.FechaLimite;
                            prestamoId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        }

                        // 2) Insertar detalles y descontar stock por cada libro
                        foreach (var det in prestamo.Detalles)
                        {
                            string sqlDet = @"INSERT INTO DetallePrestamo
                                                  (PrestamoId, LibroId, FechaDevolucion)
                                              VALUES (@p, @l, NULL)";

                            using (var cmd = new SqlCommand(sqlDet, cn, tx))
                            {
                                cmd.Parameters.Add("@p", SqlDbType.Int).Value = prestamoId;
                                cmd.Parameters.Add("@l", SqlDbType.Int).Value = det.LibroId;
                                await cmd.ExecuteNonQueryAsync();
                            }

                            string sqlStock = @"UPDATE Libros
                                                SET Ejemplares = Ejemplares - 1
                                                WHERE LibroId = @l AND Ejemplares > 0";

                            using (var cmd = new SqlCommand(sqlStock, cn, tx))
                            {
                                cmd.Parameters.Add("@l", SqlDbType.Int).Value = det.LibroId;
                                int filas = await cmd.ExecuteNonQueryAsync();
                                if (filas == 0)
                                    throw new InvalidOperationException(
                                        $"El libro {det.LibroId} no tiene ejemplares disponibles.");
                            }
                        }

                        await tx.CommitAsync();
                        return prestamoId;
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // Registra devolucion de un libro: fecha, repone stock y actualiza estado si corresponde
        public async Task<int> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fecha)
        {
            using (var cn = Conexion.Obtener())
            {
                await cn.OpenAsync();

                using (var tx = (SqlTransaction)await cn.BeginTransactionAsync())
                {
                    try
                    {
                        // 1) Marcar la devolucion en el detalle
                        string sqlDet = @"UPDATE DetallePrestamo
                                          SET FechaDevolucion = @fecha
                                          WHERE PrestamoId = @p AND LibroId = @l
                                            AND FechaDevolucion IS NULL";

                        int filas;
                        using (var cmd = new SqlCommand(sqlDet, cn, tx))
                        {
                            cmd.Parameters.Add("@fecha", SqlDbType.DateTime).Value = fecha;
                            cmd.Parameters.Add("@p", SqlDbType.Int).Value = prestamoId;
                            cmd.Parameters.Add("@l", SqlDbType.Int).Value = libroId;
                            filas = await cmd.ExecuteNonQueryAsync();
                        }

                        if (filas == 0)
                            throw new InvalidOperationException("Este libro ya fue devuelto o no corresponde al prestamo.");

                        // 2) Reponer stock
                        string sqlStock = "UPDATE Libros SET Ejemplares = Ejemplares + 1 WHERE LibroId = @l";
                        using (var cmd = new SqlCommand(sqlStock, cn, tx))
                        {
                            cmd.Parameters.Add("@l", SqlDbType.Int).Value = libroId;
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 3) Si no quedan pendientes, cambiar estado a Devuelto
                        string sqlCheck = @"SELECT COUNT(1) FROM DetallePrestamo
                                            WHERE PrestamoId = @p AND FechaDevolucion IS NULL";
                        int pendientes;
                        using (var cmd = new SqlCommand(sqlCheck, cn, tx))
                        {
                            cmd.Parameters.Add("@p", SqlDbType.Int).Value = prestamoId;
                            pendientes = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                        }

                        if (pendientes == 0)
                        {
                            string sqlEstado = "UPDATE Prestamos SET Estado = 'Devuelto' WHERE PrestamoId = @p";
                            using (var cmd = new SqlCommand(sqlEstado, cn, tx))
                            {
                                cmd.Parameters.Add("@p", SqlDbType.Int).Value = prestamoId;
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }

                        await tx.CommitAsync();
                        return filas;
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        // Obtiene prestamos pendientes de un socio (para la pantalla de devolucion)
        public async Task<List<Prestamo>> ListarPendientesPorSocioAsync(int socioId)
        {
            var lista = new List<Prestamo>();

            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT p.PrestamoId, p.SocioId, p.FechaPrestamo,
                                      p.FechaLimite, p.Estado, s.Nombre, s.DNI
                               FROM Prestamos p
                               INNER JOIN Socios s ON s.SocioId = p.SocioId
                               WHERE p.SocioId = @s AND p.Estado = 'Pendiente'
                               ORDER BY p.FechaPrestamo DESC";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@s", SqlDbType.Int).Value = socioId;

                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Prestamo
                            {
                                Id = dr.GetInt32(0),
                                SocioId = dr.GetInt32(1),
                                FechaPrestamo = dr.GetDateTime(2),
                                FechaLimite = dr.GetDateTime(3),
                                Estado = dr.GetString(4),
                                NombreSocio = dr.GetString(5),
                                DniSocio = dr.GetString(6)
                            });
                        }
                    }
                }

                // Cargar detalles pendientes de cada prestamo
                foreach (var p in lista)
                {
                    string sqlDet = @"SELECT d.PrestamoId, d.LibroId, d.FechaDevolucion, l.Titulo
                                      FROM DetallePrestamo d
                                      INNER JOIN Libros l ON l.LibroId = d.LibroId
                                      WHERE d.PrestamoId = @p AND d.FechaDevolucion IS NULL";

                    using (var cmd = new SqlCommand(sqlDet, cn))
                    {
                        cmd.Parameters.Add("@p", SqlDbType.Int).Value = p.Id;

                        using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            while (await dr.ReadAsync())
                            {
                                p.Detalles.Add(new DetallePrestamo
                                {
                                    PrestamoId = dr.GetInt32(0),
                                    LibroId = dr.GetInt32(1),
                                    FechaDevolucion = dr.IsDBNull(2) ? null : dr.GetDateTime(2),
                                    TituloLibro = dr.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            return lista;
        }

        // Reporte por intervalo de fechas (requisito 14 - INNER JOIN)
        public async Task<List<Prestamo>> ReportePorFechasAsync(DateTime desde, DateTime hasta)
        {
            var lista = new List<Prestamo>();

            using (var cn = Conexion.Obtener())
            {
                string sql = @"SELECT p.PrestamoId, p.SocioId, p.FechaPrestamo,
                                      p.FechaLimite, p.Estado, s.Nombre, s.DNI,
                                      d.LibroId, d.FechaDevolucion, l.Titulo
                               FROM Prestamos p
                               INNER JOIN Socios s          ON s.SocioId    = p.SocioId
                               INNER JOIN DetallePrestamo d ON d.PrestamoId = p.PrestamoId
                               INNER JOIN Libros l          ON l.LibroId    = d.LibroId
                               WHERE p.FechaPrestamo BETWEEN @desde AND @hasta
                               ORDER BY p.FechaPrestamo DESC, p.PrestamoId";

                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde.Date;
                    cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hasta.Date.AddDays(1).AddSeconds(-1);

                    await cn.OpenAsync();
                    using (var dr = await cmd.ExecuteReaderAsync())
                    {
                        Prestamo? actual = null;
                        while (await dr.ReadAsync())
                        {
                            int prestamoId = dr.GetInt32(0);

                            if (actual == null || actual.Id != prestamoId)
                            {
                                actual = new Prestamo
                                {
                                    Id = prestamoId,
                                    SocioId = dr.GetInt32(1),
                                    FechaPrestamo = dr.GetDateTime(2),
                                    FechaLimite = dr.GetDateTime(3),
                                    Estado = dr.GetString(4),
                                    NombreSocio = dr.GetString(5),
                                    DniSocio = dr.GetString(6)
                                };
                                lista.Add(actual);
                            }

                            actual.Detalles.Add(new DetallePrestamo
                            {
                                PrestamoId = prestamoId,
                                LibroId = dr.GetInt32(7),
                                FechaDevolucion = dr.IsDBNull(8) ? null : dr.GetDateTime(8),
                                TituloLibro = dr.GetString(9)
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}