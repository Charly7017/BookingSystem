using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingSystem.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaNumberController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villaNumbers = _db.VillaNumbers.ToList();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            var villas = _db.Villas.ToList().Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            });

            ViewBag.VillaList = villas;

            return View();
        }



        [HttpPost]
        public IActionResult Create(VillaNumber villaNumber)
        {
            ModelState.Remove("Villa");
            if (!ModelState.IsValid)
            {
                TempData["error"] = "The villa number could not be created";
                return View(villaNumber);
            }

            _db.VillaNumbers.Add(villaNumber);
            _db.SaveChanges();
            TempData["success"] = "The villa number has been created successfully";
            return RedirectToAction("Index");

        }

        public IActionResult Update(int villaId)
        {
            var obj = _db.Villas.FirstOrDefault(p=>p.Id == villaId);

            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(obj);
        }

        [HttpPost]
        public IActionResult Update(Villa villa)
        {

            if (!ModelState.IsValid)
            {
                TempData["success"] = "The villa could not be updated";
                return View(villa);
            }

            _db.Villas.Update (villa);
            _db.SaveChanges();
            TempData["success"] = "The villa has been updated successfully";
            return RedirectToAction("Index");

        }

        public IActionResult Delete(int villaId)
        {
            var obj = _db.Villas.FirstOrDefault(p => p.Id == villaId);

            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var objFromDb = _db.Villas.FirstOrDefault(p=>p.Id == villa.Id);

            if (objFromDb is null)
            {
                TempData["error"] = "The villa could not be deleted";
                return RedirectToAction("Error", "Home");
            }

            _db.Villas.Remove (objFromDb);
            _db.SaveChanges();
            TempData["success"] = "The villa has been deleted successfully";
            return RedirectToAction("Index");
        }


    }
}
