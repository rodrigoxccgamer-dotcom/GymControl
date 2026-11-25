
using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace GymControl.Data
{
    public class ReporteDAO
    {
        private readonly string _conexion;

        public ReporteDAO(IConfiguration config)
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

        public DataTable ObtenerReporteVentas(
            DateTime inicio,
            DateTime fin,
            string tipoGranularidad)
        {
            DataTable tabla = new DataTable("ReporteVentas");

            if (inicio.Date > fin.Date)
                throw new ArgumentException(
                    "La fecha inicial no puede ser mayor que la final.");

            bool mensual = tipoGranularidad.Equals(
                "Mensual",
                StringComparison.OrdinalIgnoreCase);

            if (!mensual &&
                !tipoGranularidad.Equals(
                    "Diario",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Seleccione Diario o Mensual.");
            }

            string sql;

            if (mensual)
            {
                sql = @"
                    SELECT
                        TO_CHAR(v.fecha_venta, 'YYYY-MM')
                            AS periodo_mes,

                        COUNT(v.id) AS cantidad_ventas,

                        COALESCE(SUM(v.total), 0)
                            AS total_bruto,

                        COALESCE(SUM(v.monto_descuento), 0)
                            AS total_descuentos,

                        COALESCE(SUM(v.total_final), 0)
                            AS total_cobrado

                    FROM ventas v

                    WHERE v.fecha_venta::date
                        BETWEEN @inicio::date AND @fin::date

                    GROUP BY
                        TO_CHAR(v.fecha_venta, 'YYYY-MM')

                    ORDER BY periodo_mes ASC";
            }
            else
            {
                sql = @"
                    SELECT
                        TO_CHAR(v.fecha_venta, 'YYYY-MM-DD')
                            AS periodo_grupo,

                        v.id AS nro_venta,

                        TO_CHAR(v.fecha_venta, 'HH24:MI')
                            AS hora,

                        c.nombres || ' ' || c.apellidos
                            AS cliente,

                        v.vendedor,

                        v.total AS total_bruto,

                        v.porcentaje_descuento,

                        v.monto_descuento,

                        v.total_final,

                        v.estado

                    FROM ventas v

                    INNER JOIN clientes c
                        ON c.id = v.cliente_id

                    WHERE v.fecha_venta::date
                        BETWEEN @inicio::date AND @fin::date

                    ORDER BY v.fecha_venta ASC";
            }

            try
            {
                using var conexion =
                    new NpgsqlConnection(_conexion);

                conexion.Open();

                using var comando =
                    new NpgsqlCommand(sql, conexion);

                comando.Parameters.AddWithValue(
                    "inicio", inicio.Date);

                comando.Parameters.AddWithValue(
                    "fin", fin.Date);

                using var reader = comando.ExecuteReader();

                tabla.Load(reader);

                return tabla;
            }
            catch (NpgsqlException ex)
            {
                ErrorLogger.Registrar(
                    ex,
                    "Generar reporte de ventas");

                throw new InvalidOperationException(
                    "No pudimos consultar las ventas. " +
                    "Verifique la conexión con la base de datos.",
                    ex);
            }
        }
    }
}
