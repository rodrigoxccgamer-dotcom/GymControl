
using Npgsql;
using GymControl.Models;

namespace GymControl.Data
{
    public class ClienteDAO
    {
        private readonly string _conexion;

        public ClienteDAO(IConfiguration config)
        {
            var baseConexion = config.GetConnectionString("PostgreSQL");
            var password = config["ConnectionStrings:PostgreSQLPassword"];

            var builder = new NpgsqlConnectionStringBuilder(baseConexion)
            {
                Password = password
            };

            _conexion = builder.ConnectionString;
        }

        // CREATE: Insertar cliente
        public void Insertar(Cliente cliente)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"INSERT INTO clientes
                (nombres, apellidos, ci, telefono, correo)
                VALUES (@nombres, @apellidos, @ci, @telefono, @correo)";

            using var comando = new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("nombres", cliente.Nombres);
            comando.Parameters.AddWithValue("apellidos", cliente.Apellidos);
            comando.Parameters.AddWithValue("ci", cliente.CI);
            comando.Parameters.AddWithValue("telefono",
                (object?)cliente.Telefono ?? DBNull.Value);
            comando.Parameters.AddWithValue("correo",
                (object?)cliente.Correo ?? DBNull.Value);

            comando.ExecuteNonQuery();
        }

        // READ: Obtener todos los clientes
        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();

            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"SELECT id, nombres, apellidos,
                           ci, telefono, correo, fecha_registro
                           FROM clientes ORDER BY id";

            using var comando = new NpgsqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Cliente
                {
                    Id = reader.GetInt32(0),
                    Nombres = reader.GetString(1),
                    Apellidos = reader.GetString(2),
                    CI = reader.GetString(3),
                    Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Correo = reader.IsDBNull(5) ? null : reader.GetString(5),
                    FechaRegistro = reader.GetDateTime(6)
                });
            }

            return lista;
        }

        // READ: Obtener cliente por ID
        public Cliente? ObtenerPorId(int id)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"SELECT id, nombres, apellidos,
                           ci, telefono, correo, fecha_registro
                           FROM clientes WHERE id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("id", id);

            using var reader = comando.ExecuteReader();

            if (reader.Read())
            {
                return new Cliente
                {
                    Id = reader.GetInt32(0),
                    Nombres = reader.GetString(1),
                    Apellidos = reader.GetString(2),
                    CI = reader.GetString(3),
                    Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Correo = reader.IsDBNull(5) ? null : reader.GetString(5),
                    FechaRegistro = reader.GetDateTime(6)
                };
            }

            return null;
        }

        // UPDATE: Actualizar cliente
        public void Actualizar(Cliente cliente)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"UPDATE clientes SET
                            nombres = @nombres,
                            apellidos = @apellidos,
                            ci = @ci,
                            telefono = @telefono,
                            correo = @correo
                           WHERE id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("id", cliente.Id);
            comando.Parameters.AddWithValue("nombres", cliente.Nombres);
            comando.Parameters.AddWithValue("apellidos", cliente.Apellidos);
            comando.Parameters.AddWithValue("ci", cliente.CI);
            comando.Parameters.AddWithValue("telefono",
                (object?)cliente.Telefono ?? DBNull.Value);
            comando.Parameters.AddWithValue("correo",
                (object?)cliente.Correo ?? DBNull.Value);

            comando.ExecuteNonQuery();
        }

        // DELETE: Eliminar cliente por ID
        public bool Eliminar(int id)
        {
            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = "DELETE FROM clientes WHERE id = @id";

            using var comando = new NpgsqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("id", id);

            int filasAfectadas = comando.ExecuteNonQuery();

            return filasAfectadas > 0;
        }

    }
}
