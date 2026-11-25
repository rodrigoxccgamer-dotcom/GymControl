
using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using GymControl.Data;

namespace GymControl.Controllers
{
    public class VentaReporteController : Controller
    {
        private readonly ReporteDAO _reporteDAO;

        public VentaReporteController(ReporteDAO reporteDAO)
        {
            _reporteDAO = reporteDAO;
        }

        [HttpGet]
        public IActionResult Index(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string tipoGranularidad = "Diario")
        {
            DateTime inicio = fechaInicio?.Date ??
                new DateTime(DateTime.Today.Year,
                             DateTime.Today.Month, 1);

            DateTime fin = fechaFin?.Date ?? DateTime.Today;

            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");
            ViewBag.TipoGranularidad = tipoGranularidad;

            ViewBag.Reporte = new DataTable("ReporteVentas");

            if (inicio > fin)
            {
                ViewBag.Error =
                    "La fecha inicial no puede ser mayor que la fecha final.";

                return View();
            }

            if (tipoGranularidad != "Diario" &&
                tipoGranularidad != "Mensual")
            {
                ViewBag.Error = "Seleccione un tipo de reporte válido.";
                return View();
            }

            try
            {
                DataTable reporte =
                    _reporteDAO.ObtenerReporteVentas(
                        inicio,
                        fin,
                        tipoGranularidad);

                ViewBag.Reporte = reporte;

                if (reporte.Rows.Count == 0)
                {
                    ViewBag.Mensaje =
                        "No existen ventas registradas en el rango " +
                        "de fechas seleccionado. Intente ampliar " +
                        "el periodo de búsqueda.";
                }
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "Ocurrió un problema al generar el reporte. " +
                    "Intente nuevamente.";
            }

            return View();
        }
    }
}
