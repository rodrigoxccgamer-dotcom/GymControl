
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

        [HttpGet]
        public IActionResult Index()
        {
            var ventas = _ventaDAO.ObtenerTodas();
            return View(ventas);
        }

        [HttpGet]
        public IActionResult Create()
        {
            PrepararListas();

            var venta = new Venta
            {
                FechaVenta = DateTimeOffset.Now,
                Detalles = new List<DetalleVenta>
                {
                    new DetalleVenta()
                }
            };

            return View(venta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Venta venta)
        {
            venta.Detalles = (venta.Detalles ??
                new List<DetalleVenta>())
                .Where(d => d.ProductoId > 0 && d.Cantidad > 0)
                .ToList();

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
                    $"Total: Bs {venta.Total:N2}.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un error al registrar la venta. " +
                    "Verifique los datos e intente nuevamente."
                );
            }

            PrepararListas();
            return View(venta);
        }

        private void PrepararListas()
        {
            ViewBag.Clientes = _clienteDAO.ObtenerTodos();
            ViewBag.Productos = _productoDAO.ObtenerActivos();
        }
    }
}
