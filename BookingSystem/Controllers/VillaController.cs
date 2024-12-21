﻿using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingSystem.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public VillaController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var villas = _db.Villas.ToList();
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

            _db.Villas.Add(villa);
            _db.SaveChanges();
            TempData["success"] = "The villa has been created successfully";
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
