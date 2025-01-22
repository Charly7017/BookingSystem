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
                    _unitOfWork.Booking.UpdateStatus(bookingDb.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentId(bookingDb.Id,session.Id,
                        session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }


            return View(bookingId);

        }




    }
}
