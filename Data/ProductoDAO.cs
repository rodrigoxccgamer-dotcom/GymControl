
using Npgsql;
using GymControl.Models;

namespace GymControl.Data
{
    public class ProductoDAO
    {
        private readonly string _conexion;

        public ProductoDAO(IConfiguration config)
        {
            var baseConexion = config.GetConnectionString("PostgreSQL");
            var password = config["ConnectionStrings:PostgreSQLPassword"];

            var builder = new NpgsqlConnectionStringBuilder(baseConexion)
            {
                Password = password
            };

            _conexion = builder.ConnectionString;
        }

        // READ: Obtener planes activos
        public List<Producto> ObtenerActivos()
        {
            var lista = new List<Producto>();

            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"SELECT id, nombre, descripcion,
                                  precio, duracion_dias, activo,
                                  fecha_creacion
                           FROM productos
                           WHERE activo = TRUE
                           ORDER BY precio";

            using var comando = new NpgsqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Producto
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1),
                    Descripcion = reader.IsDBNull(2)
                        ? null : reader.GetString(2),
                    Precio = reader.GetDecimal(3),
                    DuracionDias = reader.GetInt32(4),
                    Activo = reader.GetBoolean(5),
                    FechaCreacion = new DateTimeOffset(
                        reader.GetDateTime(6))
                });
            }

            return lista;
        }
    }
}
