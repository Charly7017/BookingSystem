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
        private readonly ApplicationDbContext _db;

        public VillaNumberController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villaNumbers = _db.VillaNumbers.Include(p=>p.Villa).ToList();
            return View(villaNumbers);
        }

        public IActionResult Create()
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = GetVillaNumbersSelect()
            };
                
            return View(villaNumberVM);
        }



        [HttpPost]
        public IActionResult Create(VillaNumberVM villa_number)
        {
            bool recordExists = _db.VillaNumbers.Any(p => p.Villa_Number == villa_number.VillaNumber.Villa_Number);

            if (recordExists)
            {
                TempData["error"] = "The villa number already exists";
                villa_number.VillaList = GetVillaNumbersSelect();
                return View(villa_number);
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "The villa number could not be created";
                villa_number.VillaList = GetVillaNumbersSelect();
                return View(villa_number);
            }

            _db.VillaNumbers.Add(villa_number.VillaNumber);
            _db.SaveChanges();
            TempData["success"] = "The villa number has been created successfully";
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Update(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = GetVillaNumbersSelect(),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(p=>p.Villa_Number == villaNumberId)
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
                villa_number.VillaList = GetVillaNumbersSelect();
                return View(villa_number);
            }

            _db.VillaNumbers.Update(villa_number.VillaNumber);
            _db.SaveChanges();
            TempData["success"] = "The villa number has been updated successfully";
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Delete(int villaNumberId)
        {
            var villaNumberVM = new VillaNumberVM
            {
                VillaList = GetVillaNumbersSelect(),
                VillaNumber = _db.VillaNumbers.FirstOrDefault(p => p.Villa_Number == villaNumberId)
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
            var objFromDb = _db.VillaNumbers.FirstOrDefault(p=>p.Villa_Number == villa_number.VillaNumber.Villa_Number);

            if (objFromDb is null)
            {
                TempData["error"] = "The villa number could not be deleted";
                return RedirectToAction("Error", "Home");
            }

            _db.VillaNumbers.Remove(objFromDb);
            _db.SaveChanges();
            TempData["success"] = "The villa number has been deleted successfully";
            return RedirectToAction(nameof(Index));
        }


        private IEnumerable<SelectListItem> GetVillaNumbersSelect()
        {
            var list = _db.Villas.ToList().Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            });

            return list;

        }


    }
}
