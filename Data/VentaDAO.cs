
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Npgsql;
using GymControl.Models;

namespace GymControl.Data
{
    public class VentaDAO
    {
        private readonly string _conexion;

        public VentaDAO(IConfiguration config)
        {
            var baseConexion = config.GetConnectionString("PostgreSQL");
            var password = config["ConnectionStrings:PostgreSQLPassword"];

            var builder = new NpgsqlConnectionStringBuilder(baseConexion)
            {
                Password = password
            };

            _conexion = builder.ConnectionString;
        }

        // CREATE: Registrar venta con sus detalles
        public int Registrar(Venta venta)
        {
            if (venta.ClienteId <= 0)
                throw new ArgumentException("Debe seleccionar un cliente.");

            if (venta.Detalles == null || venta.Detalles.Count == 0)
                throw new ArgumentException("Debe agregar al menos un plan.");

            if (venta.Detalles.Any(d => d.Cantidad <= 0 || d.ProductoId <= 0))
                throw new ArgumentException("Los productos y cantidades deben ser válidos.");

            if (venta.Detalles.GroupBy(d => d.ProductoId).Any(g => g.Count() > 1))
                throw new ArgumentException("No puede repetir un mismo plan en la venta.");

            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            using var transaccion = conexion.BeginTransaction();

            try
            {
                // 1. Insertar cabecera de venta
                string sqlVenta = @"
                    INSERT INTO ventas (cliente_id, total, estado)
                    VALUES (@clienteId, 0, 'Confirmada')
                    RETURNING id, fecha_venta";

                using (var comandoVenta = new NpgsqlCommand(
                    sqlVenta, conexion, transaccion))
                {
                    comandoVenta.Parameters.AddWithValue(
                        "clienteId", venta.ClienteId);

                    using var reader = comandoVenta.ExecuteReader();

                    reader.Read();

                    venta.Id = reader.GetInt32(0);

                    venta.FechaVenta = new DateTimeOffset(
                        reader.GetDateTime(1));
                }

                decimal total = 0;

                // 2. Registrar cada detalle
                foreach (var detalle in venta.Detalles)
                {
                    // Obtener precio real desde PostgreSQL
                    string sqlProducto = @"
                        SELECT nombre, precio
                        FROM productos
                        WHERE id = @productoId
                        AND activo = TRUE
                        FOR SHARE";

                    string nombreProducto;
                    decimal precio;

                    using (var comandoProducto = new NpgsqlCommand(
                        sqlProducto, conexion, transaccion))
                    {
                        comandoProducto.Parameters.AddWithValue(
                            "productoId", detalle.ProductoId);

                        using var reader = comandoProducto.ExecuteReader();

                        if (!reader.Read())
                        {
                            throw new InvalidOperationException(
                                "El plan seleccionado no existe o está inactivo.");
                        }

                        nombreProducto = reader.GetString(0);
                        precio = reader.GetDecimal(1);
                    }

                    // Calcular subtotal
                    decimal subtotal = precio * detalle.Cantidad;

                    string sqlDetalle = @"
                        INSERT INTO detalle_ventas
                        (venta_id, producto_id, cantidad,
                         precio_unitario, subtotal)
                        VALUES
                        (@ventaId, @productoId, @cantidad,
                         @precio, @subtotal)";

                    using (var comandoDetalle = new NpgsqlCommand(
                        sqlDetalle, conexion, transaccion))
                    {
                        comandoDetalle.Parameters.AddWithValue(
                            "ventaId", venta.Id);

                        comandoDetalle.Parameters.AddWithValue(
                            "productoId", detalle.ProductoId);

                        comandoDetalle.Parameters.AddWithValue(
                            "cantidad", detalle.Cantidad);

                        comandoDetalle.Parameters.AddWithValue(
                            "precio", precio);

                        comandoDetalle.Parameters.AddWithValue(
                            "subtotal", subtotal);

                        comandoDetalle.ExecuteNonQuery();
                    }

                    detalle.VentaId = venta.Id;
                    detalle.NombreProducto = nombreProducto;
                    detalle.PrecioUnitario = precio;
                    detalle.Subtotal = subtotal;

                    total += subtotal;
                }

                // 3. Actualizar total de la cabecera
                string sqlActualizar = @"
                    UPDATE ventas
                    SET total = @total
                    WHERE id = @id";

                using (var comandoActualizar = new NpgsqlCommand(
                    sqlActualizar, conexion, transaccion))
                {
                    comandoActualizar.Parameters.AddWithValue(
                        "total", total);

                    comandoActualizar.Parameters.AddWithValue(
                        "id", venta.Id);

                    comandoActualizar.ExecuteNonQuery();
                }

                // 4. Confirmar toda la operación
                transaccion.Commit();

                venta.Total = total;

                return venta.Id;
            }
            catch
            {
                // 5. Deshacer todo si ocurre un error
                transaccion.Rollback();

                throw;
            }
        }

        // READ: Obtener historial de ventas
        public List<Venta> ObtenerTodas()
        {
            var lista = new List<Venta>();

            using var conexion = new NpgsqlConnection(_conexion);
            conexion.Open();

            string sql = @"
                SELECT
                    v.id,
                    v.cliente_id,
                    c.nombres || ' ' || c.apellidos AS nombre_cliente,
                    v.fecha_venta,
                    v.total,
                    v.estado
                FROM ventas v
                INNER JOIN clientes c
                    ON c.id = v.cliente_id
                ORDER BY v.id DESC";

            using var comando = new NpgsqlCommand(sql, conexion);
            using var reader = comando.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Venta
                {
                    Id = reader.GetInt32(0),
                    ClienteId = reader.GetInt32(1),
                    NombreCliente = reader.GetString(2),
                    FechaVenta = new DateTimeOffset(
                        reader.GetDateTime(3)),
                    Total = reader.GetDecimal(4),
                    Estado = reader.GetString(5)
                });
            }

            return lista;
        }
    }
}
