
using Microsoft.AspNetCore.Mvc;
using GymControl.Data;
using GymControl.Models;
using Npgsql;

namespace GymControl.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteDAO _dao;

        public ClienteController(ClienteDAO dao)
        {
            _dao = dao;
        }

        // READ: Mostrar todos los clientes
        public IActionResult Index()
        {
            List<Cliente> clientes = _dao.ObtenerTodos();
            return View(clientes);
        }

        // CREATE: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE: Guardar cliente en PostgreSQL
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                _dao.Insertar(cliente);

                TempData["Mensaje"] = "Cliente registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CI", "Ya existe un cliente con este CI.");
                return View(cliente);
            }
        }

        // UPDATE: Mostrar formulario de edición
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Cliente? cliente = _dao.ObtenerPorId(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // UPDATE: Guardar los cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                _dao.Actualizar(cliente);

                TempData["Mensaje"] = "Cliente actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                ModelState.AddModelError("CI", "Ya existe un cliente con ese CI.");
                return View(cliente);
            }
        }

        // DELETE: Mostrar confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Cliente? cliente = _dao.ObtenerPorId(id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // DELETE: Confirmar eliminación
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                bool eliminado = _dao.Eliminar(id);

                if (!eliminado)
                {
                    return NotFound();
                }

                TempData["Mensaje"] = "Cliente eliminado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Npgsql.PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["Error"] =
                    "No se puede eliminar este cliente porque tiene membresías registradas.";

                return RedirectToAction(nameof(Index));
            }
        }

    }
}
