
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

        // READ: Listar clientes
        public IActionResult Index()
        {
            List<Cliente> clientes = _dao.ObtenerTodos();
            return View(clientes);
        }

        // CREATE: Formulario original
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // CREATE: Guardar desde formulario original
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
            catch (PostgresException ex)
                when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "CI", "Ya existe un cliente con ese CI.");

                return View(cliente);
            }
        }

        // CREATE: Guardar desde ventana modal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateModal(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _dao.Insertar(cliente);

                    TempData["Mensaje"] =
                        "Cliente registrado correctamente.";

                    return Json(new { ok = true });
                }
                catch (PostgresException ex)
                    when (ex.SqlState == "23505")
                {
                    ModelState.AddModelError(
                        "CI",
                        "Este CI ya está registrado. Ingrese otro.");
                }
            }

            var errores = ModelState
                .Where(x => x.Value != null &&
                            x.Value.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors
                        .Select(e => e.ErrorMessage)
                        .ToArray()
                );

            return BadRequest(
                new { ok = false, errors = errores });
        }

        // UPDATE: Mostrar edición
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Cliente? cliente = _dao.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
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
            catch (PostgresException ex)
                when (ex.SqlState == "23505")
            {
                ModelState.AddModelError(
                    "CI", "Ya existe un cliente con ese CI.");

                return View(cliente);
            }
        }

        // DELETE: Confirmar eliminación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Cliente? cliente = _dao.ObtenerPorId(id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        // DELETE: Eliminar registro
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
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
                    "No se puede eliminar el cliente porque tiene membresías registradas.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
