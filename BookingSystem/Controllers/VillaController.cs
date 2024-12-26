using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingSystem.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepo;

        public VillaController(IVillaRepository villaRepo)
        {
            _villaRepo = villaRepo;
        }

        public IActionResult Index()
        {
            var villas = _villaRepo.GetAll();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (!ModelState.IsValid)
            {
                TempData["error"] = "The villa could not be created";
                return View(villa);
            }

            _villaRepo.Add(villa);
            _villaRepo.Save();
            TempData["success"] = "The villa has been created successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Update(int villaId)
        {
            var obj = _villaRepo.Get(p=>p.Id==villaId);

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

            _villaRepo.Update(villa);
            _villaRepo.Save();
            TempData["success"] = "The villa has been updated successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int villaId)
        {
            var obj = _villaRepo.Get(p => p.Id == villaId);

            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var objFromDb = _villaRepo.Get(p=>p.Id == villa.Id);

            if (objFromDb is null)
            {
                TempData["error"] = "The villa could not be deleted";
                return RedirectToAction("Error", "Home");
            }

            _villaRepo.Remove(objFromDb);
            _villaRepo.Save();
            TempData["success"] = "The villa has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


    }
}
