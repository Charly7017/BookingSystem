using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

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
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");

                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\VillaImage\" + fileName;

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

            if (villa.Image is not null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\VillaImage");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(
                        _webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                    if(System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                }


                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    villa.Image.CopyTo(fileStream);

                villa.ImageUrl = @"\images\VillaImage\" + fileName;

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


            if (!string.IsNullOrEmpty(objFromDb.ImageUrl))
            {
                var oldImagePath = Path.Combine(
                       _webHostEnvironment.WebRootPath, objFromDb.ImageUrl.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }


            _unitOfWork.Villa.Remove(objFromDb);
            _unitOfWork.Save();
            TempData["success"] = "The villa has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


    }
}
