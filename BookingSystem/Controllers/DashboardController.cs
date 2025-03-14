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

        public async Task<IActionResult> GetBookingPieChartData()
        {
            var totalBookings = _unitOfWork.Booking.GetAll(p => p.BookingDate >= DateTime.Now.AddDays(-30) &&
            (p.Status != SD.StatusPending || p.Status == SD.StatusCancelled));

            var customerWithOneBooking = totalBookings.GroupBy(p => p.UserId).Where(p=>p.Count() == 1).
                Select(p=>p.Key).ToList();


            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartVM pieChartVM = new()
            {
                Labels = new string[] { "New Customer Bookings", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return Json(pieChartVM);

        }


        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.Booking.GetAll(u => u.BookingDate >= DateTime.Now.AddDays(-30) &&
              u.BookingDate.Date <= DateTime.Now)
                  .GroupBy(b => b.BookingDate.Date)
                  .Select(u => new {
                      DateTime = u.Key,
                      NewBookingCount = u.Count()
                  });

            var customerData = _unitOfWork.User.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
           u.CreatedAt.Date <= DateTime.Now)
               .GroupBy(b => b.CreatedAt.Date)
               .Select(u => new {
                   DateTime = u.Key,
                   NewCustomerCount = u.Count()
               });


            var leftJoin = bookingData.GroupJoin(customerData, booking => booking.DateTime, customer => customer.DateTime,
                (booking, customer) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewCustomerCount = customer.Select(x => x.NewCustomerCount).FirstOrDefault()
                });

            var rightJoin = customerData.GroupJoin(bookingData, customer => customer.DateTime, booking => booking.DateTime,
                (customer, booking) => new
                {
                    customer.DateTime,
                    NewBookingCount = booking.Select(x => x.NewBookingCount).FirstOrDefault(),
                    customer.NewCustomerCount
                });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();


            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newCustomerData = mergedData.Select(x => x.NewCustomerCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("dd/MM/yyyy")).ToArray();


            List<ChartData> chartDataList = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newCustomerData
                },
            };

            LineChartVM lineChartVM = new()
            {
                Categories = categories,
                Series = chartDataList
            };



            return Json(lineChartVM);
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
