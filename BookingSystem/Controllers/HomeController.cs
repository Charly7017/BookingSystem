using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
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
        public IActionResult GetVillasByDate(int nights, string checkInDate)
        {

            var parsedDate = DateOnly.TryParse(checkInDate, out var checkInDateParsed);
            

            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(p=>p.Status == SD.StatusApproved
            || p.Status == SD.StatusCheckedIn).ToList();


            foreach (var villa in villaList)
            {
                int roomAvaibale = SD.VillaRoomsAvailable_Count
                    (villa.Id,villaNumbersList, DateOnly.Parse(checkInDate),nights,bookedVillas);
                villa.IsAvailable = roomAvaibale > 0 ? true: false;
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
