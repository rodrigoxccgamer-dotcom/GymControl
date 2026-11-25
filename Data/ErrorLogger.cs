
using System;
using System.IO;
using System.Text;

namespace GymControl.Data
{
    public static class ErrorLogger
    {
        private static readonly object _lock = new();

        public static void Registrar(
            Exception excepcion,
            string operacion)
        {
            try
            {
                string ruta = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "errores_sistema.log"
                );

                string registro =
                    "========================================" +
                    Environment.NewLine +
                    $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}" +
                    Environment.NewLine +
                    $"Operación: {operacion}" +
                    Environment.NewLine +
                    $"Tipo de error: {excepcion.GetType().FullName}" +
                    Environment.NewLine +
                    $"Mensaje: {excepcion.Message}" +
                    Environment.NewLine +
                    $"Detalle técnico: {excepcion}" +
                    Environment.NewLine;

                lock (_lock)
                {
                    File.AppendAllText(
                        ruta,
                        registro,
                        Encoding.UTF8
                    );
                }
            }
            catch (Exception errorLog)
            {
                // Evitar que un fallo del registro
                // oculte la excepción original.
                Console.Error.WriteLine(
                    "No se pudo escribir el archivo de errores: "
                    + errorLog.Message
                );
            }
        }
    }
}
