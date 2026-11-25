
using Microsoft.AspNetCore.Mvc;
using GymControl.Data;
using GymControl.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GymControl.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteDAO _dao;

        public ClienteController(ClienteDAO dao)
        {
            _dao = dao;
        }

        // READ: Listar clientes
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                List<Cliente> clientes = _dao.ObtenerTodos();
                return View(clientes);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No fue posible cargar los clientes. " +
                    "Verifica la conexión e inténtalo nuevamente.";

                return View(new List<Cliente>());
            }
        }

        // CREATE: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE: Registrar cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View(cliente);

            try
            {
                _dao.Insertar(cliente);

                TempData["Mensaje"] =
                    "Cliente registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(cliente);
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "CI",
                    "Ya existe un cliente con ese CI. Verifica el número e intenta nuevamente.");

                return View(cliente);
            }
            catch (NpgsqlException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible conectar con la base de datos. Inténtalo nuevamente.");

                return View(cliente);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un problema al registrar el cliente. Verifica los datos.");

                return View(cliente);
            }
        }

        // CREATE: Registrar cliente desde ventana modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateModal(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return DevolverErroresModal();

            try
            {
                _dao.Insertar(cliente);

                return Json(new
                {
                    ok = true,
                    mensaje = "Cliente registrado correctamente."
                });
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "CI",
                    "Este CI ya está registrado. Ingrese otro.");
            }
            catch (NpgsqlException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible conectar con la base de datos. Inténtelo nuevamente.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ocurrió un problema al registrar el cliente.");
            }

            return DevolverErroresModal();
        }

        // Método auxiliar para devolver errores del modal
        private IActionResult DevolverErroresModal()
        {
            var errores = ModelState
                .Where(x => x.Value != null &&
                            x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                );

            return BadRequest(new
            {
                ok = false,
                errors = errores
            });
        }

        // UPDATE: Mostrar formulario de edición
        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                Cliente? cliente = _dao.ObtenerPorId(id);

                if (cliente == null)
                    return NotFound();

                return View(cliente);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No fue posible consultar el cliente. Inténtalo nuevamente.";

                return RedirectToAction(nameof(Index));
            }
        }

        // UPDATE: Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cliente cliente)
        {
            if (!ModelState.IsValid)
                return View(cliente);

            try
            {
                _dao.Actualizar(cliente);

                TempData["Mensaje"] =
                    "Cliente actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(cliente);
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "CI",
                    "Ya existe un cliente con ese CI.");

                return View(cliente);
            }
            catch (NpgsqlException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible conectar con la base de datos.");

                return View(cliente);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible actualizar el cliente. Inténtalo nuevamente.");

                return View(cliente);
            }
        }

        // DELETE: Mostrar confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                Cliente? cliente = _dao.ObtenerPorId(id);

                if (cliente == null)
                    return NotFound();

                return View(cliente);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "No fue posible consultar el cliente para eliminarlo.";

                return RedirectToAction(nameof(Index));
            }
        }

        // DELETE: Eliminar cliente
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                bool eliminado = _dao.Eliminar(id);

                if (!eliminado)
                    return NotFound();

                TempData["Mensaje"] =
                    "Cliente eliminado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["Error"] =
                    "No se puede eliminar este cliente porque tiene membresías, ventas o pagos relacionados.";

                return RedirectToAction(nameof(Index));
            }
            catch (NpgsqlException)
            {
                TempData["Error"] =
                    "No fue posible conectar con la base de datos para eliminar el cliente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "Ocurrió un problema al eliminar el cliente. Inténtalo nuevamente.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
