using BookingSystem.Domain.Entities;
using BookingSystem.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        //public static int VillaRoomsAvailable_Count(int villaId,
        //    List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights,
        //    List<Booking> bookings)
        //{
        //    int finalAvailableRoomForAllNights = int.MaxValue;
        //    var roomsInVilla = villaNumberList.Count(x => x.VillaId == villaId);

        //    for (int i = 0; i < nights; i++)
        //    {
        //        DateOnly currentDate = checkInDate.AddDays(i);

        //        var villasBooked = bookings
        //            .Where(u => u.VillaId == villaId &&
        //                        !(u.CheckOutDate <= currentDate || u.CheckInDate >= currentDate.AddDays(nights)))
        //            .ToList();

        //        int occupiedRooms = villasBooked.Count;
        //        int totalAvailableRooms = roomsInVilla - occupiedRooms;

        //        if (totalAvailableRooms <= 0)
        //        {
        //            return 0; // No hay habitaciones disponibles
        //        }

        //        finalAvailableRoomForAllNights = Math.Min(finalAvailableRoomForAllNights, totalAvailableRooms);
        //    }

        //    return finalAvailableRoomForAllNights;
        //}

        public static int VillaRoomsAvailable_Count(int villaId,
           List<VillaNumber> villaNumberList, DateOnly checkInDate, int nights,
          List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();

            for (int i = 0; i < nights; i++)
            {
                var villasBooked = bookings.Where(u => u.CheckInDate <= checkInDate.AddDays(i)
                && u.CheckOutDate > checkInDate.AddDays(i) && u.VillaId == villaId);

                foreach (var booking in villasBooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if (totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if (finalAvailableRoomForAllNights > totalAvailableRooms)
                    {
                        finalAvailableRoomForAllNights = totalAvailableRooms;
                    }
                }
            }

            return finalAvailableRoomForAllNights;
        }

        public static RadialBarChartDto GetRadialChartDataModel(int totalCount, double currentMonthCount,
         double prevMonthCount)
        {
            RadialBarChartDto radialBarCharVM = new();

            int increaseDecreaseRatio = 100;

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount)
                    / prevMonthCount * 100);
            }

            radialBarCharVM.TotalCount = totalCount;
            radialBarCharVM.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            radialBarCharVM.HasRatioIncreased = currentMonthCount > prevMonthCount;
            radialBarCharVM.Series = new int[] { increaseDecreaseRatio };

            return radialBarCharVM;
        }

    }
}
