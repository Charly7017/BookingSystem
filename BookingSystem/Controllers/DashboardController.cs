using BookingSystem.Application.Common.Interfaces;
using BookingSystem.Application.Common.Utility;
using BookingSystem.Application.Services.Interface;
using BookingSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookingSystem.Web.Controllers
{
    public class DashboardController : Controller
    {

        private readonly IDashboardService _dashBoardService;

        public DashboardController(IDashboardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingRadialChartData()
        {
           

            return Json(await _dashBoardService.GetTotalBookingRadialChartData());

        } 
        
        public async Task<IActionResult> GetRegisteredUserChartData()
        {
            return Json(await _dashBoardService.GetRegisteredUserChartData());
        } 
        
        public async Task<IActionResult> GetRevenueChartData()
        {
            return Json(await _dashBoardService.GetRevenueChartData());
        }

        public async Task<IActionResult> GetBookingPieChartData()
        {
            return Json(await _dashBoardService.GetBookingPieChartData());
        }


        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashBoardService.GetMemberAndBookingLineChartData());
        }
    }
}
