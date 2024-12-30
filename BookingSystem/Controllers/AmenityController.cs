using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Web.Controllers
{
    public class AmenityController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;


        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll(includeProperties: "Villa");
            return View(amenities);
        }

        public IActionResult Create()
        {
            var amenityVM = new AmenityVM
            {
                VillaList = GetAmenitysSelect()
            };

            return View(amenityVM);
        }



        [HttpPost]
        public IActionResult Create(AmenityVM villa_number)
        {

            if (!ModelState.IsValid)
            {
                TempData["error"] = "The amenity could not be created";
                villa_number.VillaList = GetAmenitysSelect();
                return View(villa_number);
            }

            _unitOfWork.Amenity.Add(villa_number.Amenity);
            _unitOfWork.Save();
            TempData["success"] = "The amenity has been created successfully";
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Update(int amenityId)
        {
            var amenityVM = new AmenityVM
            {
                VillaList = GetAmenitysSelect(),
                Amenity = _unitOfWork.Amenity.Get(p => p.Id == amenityId)
            };

            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityVM);


        }

        [HttpPost]
        public IActionResult Update(AmenityVM villa_number)
        {

            if (!ModelState.IsValid)
            {
                TempData["error"] = "The amenity could not be updated";
                villa_number.VillaList = GetAmenitysSelect();
                return View(villa_number);
            }

            _unitOfWork.Amenity.Update(villa_number.Amenity);
            _unitOfWork.Save();
            TempData["success"] = "The amenity has been updated successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int amenityId)
        {
            var amenityVM = new AmenityVM
            {
                VillaList = GetAmenitysSelect(),
                Amenity = _unitOfWork.Amenity.Get(p => p.Id == amenityId)
            };

            if (amenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(amenityVM);


        }

        [HttpPost]
        public IActionResult Delete(AmenityVM villa_number)
        {
            var objFromDb = _unitOfWork.Amenity.Get(p => p.Id == villa_number.Amenity.Id);

            if (objFromDb is null)
            {
                TempData["error"] = "The amenity could not be deleted";
                return RedirectToAction("Error", "Home");
            }

            _unitOfWork.Amenity.Remove(objFromDb);
            _unitOfWork.Save();
            TempData["success"] = "The amenity has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        private IEnumerable<SelectListItem> GetAmenitysSelect()
        {
            var list = _unitOfWork.Villa.GetAll().Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            });

            return list;

        }


    }
}
