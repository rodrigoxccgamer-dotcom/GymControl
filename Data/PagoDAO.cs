
using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;
using GymControl.Models;

namespace GymControl.Data
{
    public class PagoDAO
    {
        private readonly string _conexion;

        public PagoDAO(IConfiguration config)
        {
            var baseConexion =
                config.GetConnectionString("PostgreSQL");

            var password =
                config["ConnectionStrings:PostgreSQLPassword"];

            var builder =
                new NpgsqlConnectionStringBuilder(baseConexion)
                {
                    Password = password
                };

            _conexion = builder.ConnectionString;
        }

        // CREATE: Registrar pago
        public void Insertar(Pago pago)
        {
            using var conexion =
                new NpgsqlConnection(_conexion);

            conexion.Open();

            string sql = @"INSERT INTO pagos
                (membresia_id, monto, fecha_pago, metodo_pago)
                VALUES
                (@membresia_id, @monto, @fecha_pago, @metodo_pago)";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "membresia_id", pago.MembresiaId);

            comando.Parameters.AddWithValue(
                "monto", pago.Monto);

            comando.Parameters.Add(
                "fecha_pago", NpgsqlDbType.Date
            ).Value = pago.FechaPago.Date;

            comando.Parameters.AddWithValue(
                "metodo_pago", pago.MetodoPago);

            comando.ExecuteNonQuery();
        }

        // READ: Consultar todos los pagos
        public List<Pago> ObtenerTodos()
        {
            var lista = new List<Pago>();

            using var conexion =
                new NpgsqlConnection(_conexion);

            conexion.Open();

            string sql = @"SELECT
                            p.id,
                            p.membresia_id,
                            p.monto,
                            p.fecha_pago,
                            p.metodo_pago,
                            c.nombres || ' ' || c.apellidos,
                            m.tipo
                           FROM pagos p
                           INNER JOIN membresias m
                               ON p.membresia_id = m.id
                           INNER JOIN clientes c
                               ON m.cliente_id = c.id
                           ORDER BY p.id";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Pago
                {
                    Id = reader.GetInt32(0),
                    MembresiaId = reader.GetInt32(1),
                    Monto = reader.GetDecimal(2),
                    FechaPago = reader.GetDateTime(3),
                    MetodoPago = reader.GetString(4),
                    NombreCliente = reader.GetString(5),
                    TipoMembresia = reader.GetString(6)
                });
            }

            return lista;
        }

        // READ: Buscar pago por ID
        public Pago? ObtenerPorId(int id)
        {
            using var conexion =
                new NpgsqlConnection(_conexion);

            conexion.Open();

            string sql = @"SELECT
                            p.id,
                            p.membresia_id,
                            p.monto,
                            p.fecha_pago,
                            p.metodo_pago,
                            c.nombres || ' ' || c.apellidos,
                            m.tipo
                           FROM pagos p
                           INNER JOIN membresias m
                               ON p.membresia_id = m.id
                           INNER JOIN clientes c
                               ON m.cliente_id = c.id
                           WHERE p.id = @id";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("id", id);

            using var reader = comando.ExecuteReader();

            if (reader.Read())
            {
                return new Pago
                {
                    Id = reader.GetInt32(0),
                    MembresiaId = reader.GetInt32(1),
                    Monto = reader.GetDecimal(2),
                    FechaPago = reader.GetDateTime(3),
                    MetodoPago = reader.GetString(4),
                    NombreCliente = reader.GetString(5),
                    TipoMembresia = reader.GetString(6)
                };
            }

            return null;
        }

        // UPDATE: Modificar pago
        public void Actualizar(Pago pago)
        {
            using var conexion =
                new NpgsqlConnection(_conexion);

            conexion.Open();

            string sql = @"UPDATE pagos SET
                            membresia_id = @membresia_id,
                            monto = @monto,
                            fecha_pago = @fecha_pago,
                            metodo_pago = @metodo_pago
                           WHERE id = @id";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue(
                "id", pago.Id);

            comando.Parameters.AddWithValue(
                "membresia_id", pago.MembresiaId);

            comando.Parameters.AddWithValue(
                "monto", pago.Monto);

            comando.Parameters.Add(
                "fecha_pago", NpgsqlDbType.Date
            ).Value = pago.FechaPago.Date;

            comando.Parameters.AddWithValue(
                "metodo_pago", pago.MetodoPago);

            comando.ExecuteNonQuery();
        }

        // DELETE: Eliminar pago
        public bool Eliminar(int id)
        {
            using var conexion =
                new NpgsqlConnection(_conexion);

            conexion.Open();

            string sql =
                "DELETE FROM pagos WHERE id = @id";

            using var comando =
                new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("id", id);

            int filasAfectadas =
                comando.ExecuteNonQuery();

            return filasAfectadas > 0;
        }
    }
}
