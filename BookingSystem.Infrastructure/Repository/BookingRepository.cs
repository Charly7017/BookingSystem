using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Infrastructure.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _db;

        public BookingRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Booking entity)
        {
            _db.Bookings.Update(entity);
        }

        public void UpdateStatus(int bookingId, string bookingStatus)
        {
            var bookingDb = _db.Bookings.FirstOrDefault(p=>p.Id == bookingId);

            if (bookingDb != null)
            {
                bookingDb.Status = bookingStatus;

                if (bookingStatus == SD.StatusCheckedIn)
                {
                    bookingDb.ActualCheckInDate = DateTime.Now;
                }
                if (bookingStatus == SD.StatusCompleted)
                {
                    bookingDb.ActualCheckOutDate = DateTime.Now;
                }

            }


        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var bookingDb = _db.Bookings.FirstOrDefault(p => p.Id == bookingId);

            if (bookingDb != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    bookingDb.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    bookingDb.StripePaymentIntentId = sessionId;
                    bookingDb.PaymentDate = DateTime.Now;
                    bookingDb.IsPaymentSuccessful = true;
                }
            }
        }
    }
}
