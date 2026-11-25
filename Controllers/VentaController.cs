
using Microsoft.AspNetCore.Mvc;
using GymControl.Data;
using GymControl.Models;
using Npgsql;
using System;
using System.Linq;
using System.Collections.Generic;

namespace GymControl.Controllers
{
    public class VentaController : Controller
    {
        private readonly VentaDAO _ventaDAO;
        private readonly ClienteDAO _clienteDAO;
        private readonly ProductoDAO _productoDAO;

        public VentaController(
            VentaDAO ventaDAO,
            ClienteDAO clienteDAO,
            ProductoDAO productoDAO)
        {
            _ventaDAO = ventaDAO;
            _clienteDAO = clienteDAO;
            _productoDAO = productoDAO;
        }

        // READ: Historial de ventas
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                var ventas = _ventaDAO.ObtenerTodas();
                return View(ventas);
            }
            catch (NpgsqlException)
            {
                TempData["Error"] =
                    "No fue posible consultar las ventas. " +
                    "Verifica la conexión con la base de datos.";

                return View(new List<Venta>());
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Ocurrió un problema al consultar el historial de ventas.";

                return View(new List<Venta>());
            }
        }

        // GET: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            if (!PrepararListas())
            {
                TempData["Error"] =
                    "No pudimos cargar los clientes y planes. " +
                    "Intenta nuevamente en unos momentos.";

                return RedirectToAction(nameof(Index));
            }

            var venta = new Venta
            {
                FechaVenta = DateTimeOffset.Now,
                PorcentajeDescuento = 0,
                Vendedor = string.Empty,
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta()
                }
            };

            return View(venta);
        }

        // POST: Registrar venta con descuento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Venta venta)
        {
            if (venta == null)
                return BadRequest();

            venta.Detalles = (venta.Detalles ??
                new List<DetalleVenta>())
                .Where(d => d != null &&
                            d.ProductoId > 0 &&
                            d.Cantidad > 0)
                .ToList();

            // Validar vendedor
            if (string.IsNullOrWhiteSpace(venta.Vendedor))
            {
                ModelState.AddModelError(
                    nameof(Venta.Vendedor),
                    "Ingresa el nombre del vendedor para registrar la venta.");
            }
            else if (venta.Vendedor.Trim().Length > 120)
            {
                ModelState.AddModelError(
                    nameof(Venta.Vendedor),
                    "El nombre del vendedor no puede superar 120 caracteres.");
            }
            else
            {
                venta.Vendedor = venta.Vendedor.Trim();
            }

            // Validar descuento
            if (venta.PorcentajeDescuento < 0 ||
                venta.PorcentajeDescuento > 20)
            {
                ModelState.AddModelError(
                    nameof(Venta.PorcentajeDescuento),
                    "El descuento debe estar entre 0% y 20%. " +
                    "Puedes ajustar el porcentaje e intentarlo nuevamente.");
            }

            if (!ModelState.IsValid)
            {
                PrepararListas();

                return View(venta);
            }

            try
            {
                int id = _ventaDAO.Registrar(venta);

                TempData["Mensaje"] =
                    $"Venta #{id} registrada correctamente. " +
                    $"Vendedor: {venta.Vendedor}. " +
                    $"Total bruto: Bs {venta.Total:N2}. " +
                    $"Descuento: Bs {venta.MontoDescuento:N2}. " +
                    $"Total final: Bs {venta.TotalFinal:N2}.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);
            }
            catch (PostgresException ex)
            {
                string mensaje = ex.SqlState switch
                {
                    "23503" =>
                        "El cliente o plan seleccionado ya no está disponible.",

                    "23514" =>
                        "Los datos de la venta no cumplen las reglas permitidas.",

                    "23502" =>
                        "Falta completar un dato obligatorio.",

                    "23505" =>
                        "Se detectó un registro duplicado.",

                    _ =>
                        "La base de datos no pudo completar la venta. " +
                        "Verifica los datos e inténtalo nuevamente."
                };

                ModelState.AddModelError(string.Empty, mensaje);
            }
            catch (NpgsqlException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible conectar con la base de datos. " +
                    "Verifica el servicio e inténtalo nuevamente.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un problema al registrar la venta. " +
                    "Tus datos no pudieron procesarse. Inténtalo nuevamente.");
            }

            PrepararListas();

            return View(venta);
        }

        // Preparar listas para los formularios
        private bool PrepararListas()
        {
            try
            {
                ViewBag.Clientes = _clienteDAO.ObtenerTodos();
                ViewBag.Productos = _productoDAO.ObtenerActivos();

                return true;
            }
            catch (Exception ex)
            {
                ErrorLogger.Registrar(
                    ex,
                    "Cargar clientes y planes para venta");

                ViewBag.Clientes = new List<Cliente>();
                ViewBag.Productos = new List<Producto>();

                return false;
            }
        }
    }
}
