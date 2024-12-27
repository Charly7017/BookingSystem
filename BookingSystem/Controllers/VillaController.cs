using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingSystem.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
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



            if (villa.Image is not null)
            {

            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.Villa.Add(villa);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been created successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Update(int villaId)
        {
            var obj = _unitOfWork.Villa.Get(p=>p.Id==villaId);

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

            _unitOfWork.Villa.Update(villa);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been updated successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int villaId)
        {
            var obj = _unitOfWork.Villa.Get(p => p.Id == villaId);

            if (obj is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(obj);
        }

        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var objFromDb = _unitOfWork.Villa.Get(p=>p.Id == villa.Id);

            if (objFromDb is null)
            {
                TempData["error"] = "The villa could not be deleted";
                return RedirectToAction("Error", "Home");
            }

            _unitOfWork.Villa.Remove(objFromDb);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


    }
}
