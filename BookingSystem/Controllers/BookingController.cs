using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
using BookingSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
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
        public IActionResult Index()
        {
            return View();
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


            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(p => p.Status == SD.StatusApproved
            || p.Status == SD.StatusCheckedIn).ToList();


          
           int roomAvailable = SD.VillaRoomsAvailable_Count
                (villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);


            if (roomAvailable == 0)
            {
                TempData["Error"] = "Room has been sold out";
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = villa.Id,
                    checkInDate = booking.CheckInDate,
                    nights = booking.Nights
                });
            }
            


            _unitOfWork.Booking.Add(booking);
            _unitOfWork.Save();


            var domain = Request.Scheme + "://" + Request.Host.Value + "/";

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights={booking.Nights}"
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions()
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions()
                    {
                        Name = villa.Name,
                        //Images = new List<string>
                        //{
                        //    domain + villa.ImageUrl,
                        //}
                    }
                },
                Quantity = 1,
            });


            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdateStripePaymentId(booking.Id,session.Id,session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);

        }


        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {

            var bookingDb = _unitOfWork.Booking.Get(p=>p.Id== bookingId
            ,includeProperties: "User,Villa");

            if (bookingDb.Status == SD.StatusPending)
            {
                var service = new SessionService();
                Session session = service.Get(bookingDb.StripeSessionId);
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(bookingDb.Id, SD.StatusApproved,0);
                    _unitOfWork.Booking.UpdateStripePaymentId(bookingDb.Id,session.Id,
                        session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }
            return View(bookingId);
        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Booking.Get(p=>p.Id == bookingId,includeProperties:"User,Villa");

            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumber = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll(p => p.VillaId == bookingFromDb.VillaId &&
                 availableVillaNumber.Any(x => x == p.Villa_Number)).ToList();

            }

            return View(bookingFromDb);

        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id,SD.StatusCheckedIn,booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id,SD.StatusCompleted,booking.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Booking.UpdateStatus(booking.Id,SD.StatusCancelled,0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Cancelled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }



        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll(p=>p.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Booking.GetAll(p => p.VillaId == villaId && p.Status == SD.StatusCheckedIn)
                .Select(p=>p.VillaNumber);

            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }






        #region API CALLS
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking
                    .GetAll(p=>p.UserId == userId,includeProperties:"User,Villa");

            }

            if (!string.IsNullOrEmpty(status))
            {
                objBookings = objBookings.Where(p=>p.Status.ToLower().Equals(status.ToLower()));
            }


            return Json(new { data = objBookings });
        }   
        #endregion




    }
}
