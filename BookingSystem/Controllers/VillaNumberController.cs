using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Services.Implementation;
using BookingSystem.Application.Services.Interface;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Web.Controllers
{
    public class VillaNumberController : Controller
    {

        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;

        public VillaNumberController(IVillaNumberService villaNumberService, IVillaService villaService)
        {
            _villaNumberService = villaNumberService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villaNumbers = _villaNumberService.GetAllVillaNumbers();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
                
            return View(villaNumberVM);
        }



        [HttpPost]
        public IActionResult Create(VillaNumberVM villa_number)
        {
            bool recordExists = _villaNumberService.CheckVillaNumberExists(villa_number.VillaNumber.Villa_Number);


            if (ModelState.IsValid && recordExists == false)
            {
                _villaNumberService.CreateVillaNumber(villa_number.VillaNumber);
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (recordExists)
            {
                TempData["error"] = "The villa Number already exists.";
            }


            villa_number.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            return View(villa_number);

        }


        public IActionResult Update(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };

            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);


        }

        [HttpPost]
        public IActionResult Update(VillaNumberVM villa_number)
        {

            if (!ModelState.IsValid)
            {
                TempData["error"] = "The villa number could not be updated";
                villa_number.VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                return View(villa_number);
            }

            _villaNumberService.UpdateVillaNumber(villa_number.VillaNumber);
            TempData["success"] = "The villa number has been updated successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = _villaService.GetAllVillas().Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                VillaNumber = _villaNumberService.GetVillaNumberById(villaNumberId)
            };

            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }

            return View(villaNumberVM);


        }

        [HttpPost]
        public IActionResult Delete(VillaNumberVM villa_number)
        {
            var objFromDb = _villaNumberService.GetVillaNumberById(villa_number.VillaNumber.Villa_Number);

            if (objFromDb is null)
            {
                TempData["error"] = "The villa number could not be deleted";
                return RedirectToAction("Error", "Home");
            }
            _villaNumberService.DeleteVillaNumber(objFromDb.Villa_Number);
            TempData["success"] = "The villa number has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


  
    }
}
