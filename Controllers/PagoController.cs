
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GymControl.Data;
using GymControl.Models;
using Npgsql;

namespace GymControl.Controllers
{
    public class PagoController : Controller
    {
        private readonly PagoDAO _dao;
        private readonly MembresiaDAO _membresiaDAO;

        public PagoController(
            PagoDAO dao,
            MembresiaDAO membresiaDAO)
        {
            _dao = dao;
            _membresiaDAO = membresiaDAO;
        }

        // READ: Mostrar pagos
        public IActionResult Index()
        {
            List<Pago> pagos = _dao.ObtenerTodos();
            return View(pagos);
        }

        // Cargar membresías para los formularios
        private void CargarMembresias(int? seleccionada = null)
        {
            var membresias = _membresiaDAO.ObtenerTodos();

            ViewBag.Membresias = new SelectList(
                membresias.Select(m => new
                {
                    Id = m.Id,
                    Descripcion = m.NombreCliente
                        + " - " + m.Tipo
                        + " (Bs " + m.Precio.ToString("N2") + ")"
                }),
                "Id",
                "Descripcion",
                seleccionada
            );
        }

        // Validar el método de pago
        private void ValidarPago(Pago pago)
        {
            if (pago.MetodoPago != "Efectivo" &&
                pago.MetodoPago != "Transferencia" &&
                pago.MetodoPago != "Tarjeta")
            {
                ModelState.AddModelError(
                    "MetodoPago",
                    "Seleccione un método de pago válido.");
            }
        }

        // CREATE: Mostrar formulario
        [HttpGet]
        public IActionResult Create()
        {
            CargarMembresias();
            return View(new Pago());
        }

        // CREATE: Guardar nuevo pago
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Pago pago)
        {
            ValidarPago(pago);

            if (!ModelState.IsValid)
            {
                CargarMembresias(pago.MembresiaId);
                return View(pago);
            }

            try
            {
                _dao.Insertar(pago);

                TempData["Mensaje"] =
                    "Pago registrado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "MembresiaId",
                    "La membresía seleccionada no existe.");

                CargarMembresias(pago.MembresiaId);
                return View(pago);
            }
        }

        // UPDATE: Mostrar formulario de edición
        [HttpGet]
        public IActionResult Edit(int id)
        {
            Pago? pago = _dao.ObtenerPorId(id);

            if (pago == null)
            {
                return NotFound();
            }

            CargarMembresias(pago.MembresiaId);
            return View(pago);
        }

        // UPDATE: Guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Pago pago)
        {
            ValidarPago(pago);

            if (!ModelState.IsValid)
            {
                CargarMembresias(pago.MembresiaId);
                return View(pago);
            }

            try
            {
                _dao.Actualizar(pago);

                TempData["Mensaje"] =
                    "Pago actualizado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                ModelState.AddModelError(
                    "MembresiaId",
                    "La membresía seleccionada no existe.");

                CargarMembresias(pago.MembresiaId);
                return View(pago);
            }
        }

        // DELETE: Mostrar confirmación
        [HttpGet]
        public IActionResult Delete(int id)
        {
            Pago? pago = _dao.ObtenerPorId(id);

            if (pago == null)
            {
                return NotFound();
            }

            return View(pago);
        }

        // DELETE: Confirmar eliminación
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            bool eliminado = _dao.Eliminar(id);

            if (!eliminado)
            {
                return NotFound();
            }

            TempData["Mensaje"] =
                "Pago eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
