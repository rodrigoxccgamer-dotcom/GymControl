
using Npgsql;
using GymControl.Models;

namespace GymControl.Data
{
    public class MembresiaDAO
    {
        private readonly string _conexion;

        public MembresiaDAO(IConfiguration config)
        {
            var baseConexion = config.GetConnectionString("PostgreSQL");
            var password = config["ConnectionStrings:PostgreSQLPassword"];

            var builder = new NpgsqlConnectionStringBuilder(baseConexion)
            {
                Password = password
            };

            _conexion = builder.ConnectionString;
        }
        public void Insertar(Membresia membresia)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"INSERT INTO membresias
                (cliente_id, tipo, precio, fecha_inicio, fecha_fin, estado)
                VALUES
                (@cliente_id, @tipo, @precio, @fecha_inicio, @fecha_fin, @estado)";

            using var comando = new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("cliente_id", membresia.ClienteId);
            comando.Parameters.AddWithValue("tipo", membresia.Tipo);
            comando.Parameters.AddWithValue("precio", membresia.Precio);
            comando.Parameters.AddWithValue("fecha_inicio", membresia.FechaInicio);
            comando.Parameters.AddWithValue("fecha_fin", membresia.FechaFin);
            comando.Parameters.AddWithValue("estado", membresia.Estado);

            comando.ExecuteNonQuery();
        }
        public List<Membresia> ObtenerTodos()
        {
            var lista = new List<Membresia>();

            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"SELECT m.id, m.cliente_id, m.tipo,
                                  m.precio, m.fecha_inicio, m.fecha_fin,
                                  m.estado,
                                  c.nombres || ' ' || c.apellidos AS nombre_cliente
                           FROM membresias m
                           INNER JOIN clientes c ON m.cliente_id = c.id
                           ORDER BY m.id";

            using var comando = new NpgsqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Membresia
                {
                    Id = reader.GetInt32(0),
                    ClienteId = reader.GetInt32(1),
                    Tipo = reader.GetString(2),
                    Precio = reader.GetDecimal(3),
                    FechaInicio = reader.GetDateTime(4),
                    FechaFin = reader.GetDateTime(5),
                    Estado = reader.GetString(6),
                    NombreCliente = reader.GetString(7)
                });
            }

            return lista;
        }
        public Membresia? ObtenerPorId(int id)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"SELECT m.id, m.cliente_id, m.tipo,
                                  m.precio, m.fecha_inicio, m.fecha_fin,
                                  m.estado,
                                  c.nombres || ' ' || c.apellidos AS nombre_cliente
                           FROM membresias m
                           INNER JOIN clientes c ON m.cliente_id = c.id
                           WHERE m.id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("id", id);

            using var reader = comando.ExecuteReader();

            if (reader.Read())
            {
                return new Membresia
                {
                    Id = reader.GetInt32(0),
                    ClienteId = reader.GetInt32(1),
                    Tipo = reader.GetString(2),
                    Precio = reader.GetDecimal(3),
                    FechaInicio = reader.GetDateTime(4),
                    FechaFin = reader.GetDateTime(5),
                    Estado = reader.GetString(6),
                    NombreCliente = reader.GetString(7)
                };
            }

            return null;
        }
        public void Actualizar(Membresia membresia)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"UPDATE membresias SET
                            cliente_id = @cliente_id,
                            tipo = @tipo,
                            precio = @precio,
                            fecha_inicio = @fecha_inicio,
                            fecha_fin = @fecha_fin,
                            estado = @estado
                           WHERE id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("id", membresia.Id);
            comando.Parameters.AddWithValue("cliente_id", membresia.ClienteId);
            comando.Parameters.AddWithValue("tipo", membresia.Tipo);
            comando.Parameters.AddWithValue("precio", membresia.Precio);
            comando.Parameters.AddWithValue("fecha_inicio", membresia.FechaInicio);
            comando.Parameters.AddWithValue("fecha_fin", membresia.FechaFin);
            comando.Parameters.AddWithValue("estado", membresia.Estado);

            comando.ExecuteNonQuery();
        }
        public bool Eliminar(int id)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = "DELETE FROM membresias WHERE id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("id", id);

            int filasAfectadas = comando.ExecuteNonQuery();

            return filasAfectadas > 0;
        }
    }
}
