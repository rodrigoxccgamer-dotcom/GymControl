
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymControl.Data;
using GymControl.Models;
using Npgsql;

namespace GymControl.Controllers
{
    public class MembresiaController : Controller
    {
        private readonly MembresiaDAO _dao;
        private readonly ClienteDAO _clienteDAO;

        public MembresiaController(
            MembresiaDAO dao,
            ClienteDAO clienteDAO)
        {
            _dao = dao;
            _clienteDAO = clienteDAO;
        }

        // READ: Mostrar membresías
        public IActionResult Index()
        {
            List<Membresia> membresias = _dao.ObtenerTodos();
            return View(membresias);
        }

        // Cargar lista de clientes
        private void CargarClientes(int? seleccionado = null)
        {
            var clientes = _clienteDAO.ObtenerTodos();

            ViewBag.Clientes = new SelectList(
                clientes.Select(c => new
                {
                    Id = c.Id,
                    Nombre = c.Nombres + " " + c.Apellidos
                }),
                "Id",
                "Nombre",
                seleccionado
            );
        }

        // CREATE: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            CargarClientes();
            return View(new Membresia());
        }

        // CREATE: Guardar membresía
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Membresia membresia)
        {
            ValidarMembresia(membresia);

            if (!ModelState.IsValid)
            {
                CargarClientes(membresia.ClienteId);
                return View(membresia);
            }

            try
            {
                _dao.Insertar(membresia);

                TempData["Mensaje"] =
                    "Membresía registrada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "ClienteId",
                    "El cliente seleccionado no existe.");

                CargarClientes(membresia.ClienteId);
                return View(membresia);
            }
        }

        // Validaciones de membresía
        private void ValidarMembresia(Membresia membresia)
        {
            if (membresia.FechaFin < membresia.FechaInicio)
            {
                ModelState.AddModelError(
                    "FechaFin",
                    "La fecha final no puede ser anterior a la inicial.");
            }

            if (membresia.Tipo != "Mensual" &&
                membresia.Tipo != "Trimestral" &&
                membresia.Tipo != "Anual")
            {
                ModelState.AddModelError(
                    "Tipo",
                    "Seleccione un tipo de membresía válido.");
            }

            if (membresia.Estado != "Activa" &&
                membresia.Estado != "Vencida" &&
                membresia.Estado != "Cancelada")
            {
                ModelState.AddModelError(
                    "Estado",
                    "Seleccione un estado válido.");
            }
        }

        // UPDATE: Mostrar formulario de edición
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Membresia? membresia = _dao.ObtenerPorId(id);

            if (membresia == null)
            {
                return NotFound();
            }

            CargarClientes(membresia.ClienteId);
            return View(membresia);
        }

        // UPDATE: Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Membresia membresia)
        {
            ValidarMembresia(membresia);

            if (!ModelState.IsValid)
            {
                CargarClientes(membresia.ClienteId);
                return View(membresia);
            }

            try
            {
                _dao.Actualizar(membresia);

                TempData["Mensaje"] =
                    "Membresía actualizada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "ClienteId",
                    "El cliente seleccionado no existe.");

                CargarClientes(membresia.ClienteId);
                return View(membresia);
            }
        }

        // DELETE: Mostrar confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Membresia? membresia = _dao.ObtenerPorId(id);

            if (membresia == null)
            {
                return NotFound();
            }

            return View(membresia);
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

                TempData["Mensaje"] =
                    "Membresía eliminada correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                TempData["Error"] =
                    "No se puede eliminar esta membresía porque tiene pagos registrados.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
