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

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(p => p.Status != SD.StatusPending
            || p.Status == SD.StatusCancelled);


            var countByCurrentMonth = totalBookings.Count
                (p=>p.BookingDate >= currentMonthStartDate && p.BookingDate<=DateTime.Now);

            var countByPreviousMonth = totalBookings.Count
             (p => p.BookingDate >= previousMonthStartDate && p.BookingDate <= currentMonthStartDate);


            var radialBarChartVM = GetRadialChartDataModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);


            return Json(radialBarChartVM);

        } 
        
        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            var totalUsers = _unitOfWork.User.GetAll();


            var countByCurrentMonth = totalUsers.Count
                (p=>p.CreatedAt >= currentMonthStartDate && p.CreatedAt <= DateTime.Now);

            var countByPreviousMonth = totalUsers.Count
             (p => p.CreatedAt >= previousMonthStartDate && p.CreatedAt <= currentMonthStartDate);

            var radialBarChartVM = GetRadialChartDataModel(totalUsers.Count(),countByCurrentMonth, countByPreviousMonth);

            return Json(radialBarChartVM);

        } 
        
        public async Task<IActionResult> GetRevenueChartData()
        {

            var totalBookings = _unitOfWork.Booking.GetAll(p => p.Status != SD.StatusPending
           || p.Status == SD.StatusCancelled);

            var totalRevenue = Convert.ToInt32(totalBookings.Sum(p => p.TotalCost));


            var countByCurrentMonth = totalBookings.Where
                (p => p.BookingDate >= currentMonthStartDate && p.BookingDate <= DateTime.Now).Sum(p=>p.TotalCost);

            var countByPreviousMonth = totalBookings.Where
             (p => p.BookingDate >= previousMonthStartDate && p.BookingDate <= currentMonthStartDate).Sum(p => p.TotalCost);


            var radialBarChartVM = GetRadialChartDataModel(totalRevenue,countByCurrentMonth, countByPreviousMonth);

            return Json(radialBarChartVM);

        }

        private static RadialBarChartVM GetRadialChartDataModel(int totalCount,double currentMonthCount,
            double prevMonthCount)
        {
            RadialBarChartVM radialBarCharVM = new();

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
