using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Models;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        public IActionResult Index()
        {
            var homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights,string checkInDate)
        {

            var parsedDate = DateOnly.TryParse(checkInDate, out var checkInDateParsed);


            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();


            foreach (var villa in villaList)
            {
                if (villa.Id % 2 == 0)
                {
                    villa.IsAvailable = false;
                }
            }

            var homeVM = new HomeVM()
            {
                CheckInDate = checkInDateParsed,
                //CheckOutDate = checkInDate.AddDays(nights),
                VillaList = villaList,
                Nights = nights
            };

            return PartialView("_VillaList",homeVM);

        }



        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
