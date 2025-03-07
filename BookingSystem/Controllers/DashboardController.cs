using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1? 12 : DateTime.Now.Month - 1;
        readonly DateTime previousMonthStartDate = new (DateTime.Now.Year, previousMonth,1);
        readonly DateTime currentMonthStartDate = new (DateTime.Now.Year,DateTime.Now.Month,1);


        public DashboardController(IUnitOfWork unitOfWork) 
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(p => p.Status != SD.StatusPending
            || p.Status == SD.StatusCancelled);


            var countByCurrentMonth = totalBookings.Count
                (p=>p.BookingDate >= currentMonthStartDate && p.BookingDate<=DateTime.Now);

            var countByPreviousMonth = totalBookings.Count
             (p => p.BookingDate >= previousMonthStartDate && p.BookingDate <= currentMonthStartDate);


            RadialBarChartVM radialBarCharVM = new();

            int increaseDecreaseRatio = 100;

            if (previousMonth != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((countByCurrentMonth-countByPreviousMonth)
                    /countByPreviousMonth * 100);
            }

            radialBarCharVM.TotalCount = totalBookings.Count();
            radialBarCharVM.CountInCurrentMonth = countByCurrentMonth;
            radialBarCharVM.HasRatioIncreased = currentMonthStartDate > previousMonthStartDate;
            radialBarCharVM.Series = new int[] { increaseDecreaseRatio };

            return Json(radialBarCharVM);

        }


    }
}
