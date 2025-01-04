using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingSystem.Web.Controllers
{
     
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId,string checkInDate
                    , int nights)
        {

            var parsedDate = DateOnly.TryParse(checkInDate, out var checkInDateParsed);


            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _unitOfWork.User.Get(p=>p.Id == userId);


            var booking = new Booking()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(p=>p.Id==villaId,includeProperties:"VillaAmenity"),
                CheckInDate = checkInDateParsed,
                Nights = nights,
                CheckOutDate = checkInDateParsed.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            };

            booking.TotalCost = booking.Villa.Price * nights;


            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villa.Get(p => p.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;
            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();


            return RedirectToAction(nameof(BookingConfirmation),new {bookingId = booking.Id});


        }


        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {

            return View(bookingId);

        }




    }
}
