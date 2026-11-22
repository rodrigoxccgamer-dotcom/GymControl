
using Microsoft.AspNetCore.Mvc;
using GymControl.Data;
using GymControl.Models;
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
            var ventas = _ventaDAO.ObtenerTodas();
            return View(ventas);
        }

        // GET: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            PrepararListas();

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
            venta.Detalles = (venta.Detalles ??
                new List<DetalleVenta>())
                .Where(d => d.ProductoId > 0 && d.Cantidad > 0)
                .ToList();

            // Validar vendedor
            if (string.IsNullOrWhiteSpace(venta.Vendedor))
            {
                ModelState.AddModelError(
                    nameof(Venta.Vendedor),
                    "Ingresa el nombre del vendedor para registrar la venta."
                );
            }
            else if (venta.Vendedor.Trim().Length > 120)
            {
                ModelState.AddModelError(
                    nameof(Venta.Vendedor),
                    "El nombre del vendedor no puede superar 120 caracteres."
                );
            }

            // Validar porcentaje de descuento
            if (venta.PorcentajeDescuento < 0 ||
                venta.PorcentajeDescuento > 20)
            {
                ModelState.AddModelError(
                    nameof(Venta.PorcentajeDescuento),
                    "El descuento debe estar entre 0% y 20%. " +
                    "Puedes ajustar el porcentaje e intentarlo nuevamente."
                );
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
                    ex.Message
                );
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No pudimos registrar la venta. " +
                    "Verifica los datos e inténtalo nuevamente."
                );
            }

            PrepararListas();
            return View(venta);
        }

        // Preparar listas para los formularios
        private void PrepararListas()
        {
            ViewBag.Clientes = _clienteDAO.ObtenerTodos();
            ViewBag.Productos = _productoDAO.ObtenerActivos();
        }
    }
}
