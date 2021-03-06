﻿using EbaySalesTracker.Bll;
using EbaySalesTracker.Models;
using EbaySalesTracker.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using System;
using System.Web.Mvc;

namespace EbaySalesTracker.Controllers
{
    [Authorize]
    public class DataServiceController : Controller
    {
        //Will be used for ajax calls from dashboard page

        IListingRepository _ListingRepository;
        IListingDetailRepository _ListingDetailRepository;
        IInventoryRepository _InventoryRepository;
        IInventoryBll _InventoryBll;
        ApplicationDbContext ApplicationDbContext;
        UserManager<ApplicationUser> UserManager;

        public DataServiceController() : this(null,null,null,null)
        {
        }

        public DataServiceController(IListingRepository listingRepo, IListingDetailRepository listingDetailRepo,
            IInventoryRepository inventoryRepo, IInventoryBll inventoryBll)
        {
            ApplicationDbContext = new ApplicationDbContext();
            _ListingRepository = listingRepo ?? ModelContainer.Instance.Resolve<IListingRepository>();
            _ListingDetailRepository = listingDetailRepo ?? ModelContainer.Instance.Resolve<IListingDetailRepository>();
            _InventoryRepository = inventoryRepo ?? ModelContainer.Instance.Resolve<IInventoryRepository>();
            _InventoryBll = inventoryBll ?? new InventoryBll();
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ApplicationDbContext));
        }

        // GET: DataService
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetInventoryItems()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return Json(_InventoryBll.GetInventoryItemsByUser(user.Id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetInventoryItemById(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());         
            return Json(_InventoryBll.GetInventoryItemById(id,user.Id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemSalesDataByMonth(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            return Json(_InventoryRepository.CalculateItemProfitByMonth(id,user.Id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetDataForAverageProfitOverTime(int id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var data = _InventoryBll.GetListingDataByInventoryItem(id, user.Id);
            //test = EbaySalesTracker.Builders.DashboardBuilder.AverageProfitOverTime(user.Id, id);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetBestSellingItem()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var bestSellingItem = _InventoryBll.GetBestSellingItem(user.Id);
            return Json(bestSellingItem, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetHighestAverageProfitItem()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var highestAvgProfitItem = _InventoryBll.GetHighestAverageProfitItem(user.Id);
            return Json(highestAvgProfitItem, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProfitByMonth(int year, int month)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var monthlyProfit = _InventoryBll.GetProfitByMonth(year, month, user.Id);
            return Json(monthlyProfit, JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult GetSalesByMonth(int year, int month)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var monthlySales = _InventoryBll.GetSalesByMonth(year, month, user.Id);
            return Json(monthlySales, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSalesByYear(int year)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var yearlySales = _InventoryBll.GetSalesByYear(year, user.Id);
            return Json(yearlySales, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSalesSummaries()
        {
            var today = DateTime.Now;
            var user = UserManager.FindById(User.Identity.GetUserId());

            var salesLastSevenDays = _InventoryBll.GetSalesLastSevenDays(user.Id);
            var salesThisMonth = _InventoryBll.GetSalesByMonth(today.Year, today.Month, user.Id);
            var salesThisYear = _InventoryBll.GetSalesByYear(today.Year, user.Id);
           
            return Json(new { salesLastSevenDays, salesThisMonth, salesThisYear }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProfitSummaries()
        {
            var today = DateTime.Now;
            var user = UserManager.FindById(User.Identity.GetUserId());

            var profitLastSevenDays = _InventoryBll.GetProfitLastSevenDays(user.Id);
            var profitThisMonth = _InventoryBll.GetProfitByMonth(today.Year, today.Month, user.Id);
            var profitThisYear = _InventoryBll.GetProfitByYear(today.Year, user.Id);

            return Json(new { profitLastSevenDays, profitThisMonth, profitThisYear }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFeesSummaries()
        {
            var today = DateTime.Now;
            var user = UserManager.FindById(User.Identity.GetUserId());

            var feesLastSevenDays = _InventoryBll.GetFeesLastSevenDays(user.Id);
            var feesThisMonth = _InventoryBll.GetFeesByMonth(today.Year, today.Month, user.Id);
            var feesThisYear = _InventoryBll.GetFeesByYear(today.Year, user.Id);

            return Json(new { feesLastSevenDays, feesThisMonth, feesThisYear }, JsonRequestBehavior.AllowGet);
        }
    }
}