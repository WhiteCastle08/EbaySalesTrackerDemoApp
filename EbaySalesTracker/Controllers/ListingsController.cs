﻿using EbaySalesTracker.Bll;
using EbaySalesTracker.Helpers;
using EbaySalesTracker.Models;
using EbaySalesTracker.Repository;
using EbaySalesTracker.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Practices.Unity;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace EbaySalesTracker.Controllers
{
    [Authorize]
    public class ListingsController : Controller
    {
        //private EbaySalesTrackerContext db = new EbaySalesTrackerContext();
        IListingRepository _ListingRepository;
        IListingDetailRepository _ListingDetailRepository;
        IInventoryRepository _InventoryRepository;
        IInventoryBll _InventoryBll;
        IListingBll _ListingBll;
        ApplicationDbContext ApplicationDbContext;
        UserManager<ApplicationUser> UserManager;

        

        public ListingsController() : this(null,null,null,null,null)
        {                
        }

        public ListingsController(IListingRepository listingRepo, IListingDetailRepository listingDetailRepo, IInventoryRepository inventoryRepo, IInventoryBll inventoryBll, IListingBll listingBll)
        {
            this.ApplicationDbContext = new ApplicationDbContext();
            _ListingRepository = listingRepo ?? ModelContainer.Instance.Resolve<IListingRepository>();
            _ListingDetailRepository = listingDetailRepo ?? ModelContainer.Instance.Resolve<IListingDetailRepository>();
            _InventoryRepository = inventoryRepo ?? ModelContainer.Instance.Resolve<IInventoryRepository>();
            _InventoryBll = inventoryBll ?? new InventoryBll();
            _ListingBll = listingBll ?? new ListingBll();
           
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }

        

        // GET: Listings
        public ActionResult Index(int top=10, int skip=0)
        {
            
            var user = UserManager.FindById(User.Identity.GetUserId());
            string userId = user.Id;
            var listingsViewModel = new ListingsViewModel();
            listingsViewModel.IsAuthenticatedWithEbay = false;
            listingsViewModel.Listings = _InventoryBll.GetListingsByUser(top, skip, userId).ToList();
            listingsViewModel.TotalListings = _ListingBll.GetListingsCountByUser(userId);
            listingsViewModel.Items = _InventoryBll.GetInventoryItemsByUser(userId).ToList();
            if (!string.IsNullOrEmpty(user.UserToken))
            {
                listingsViewModel.IsAuthenticatedWithEbay = true;
            }

            return View(listingsViewModel);
        }

        // GET: Listings/Details/5
        public ActionResult Details(long id)
        {
            Listing listing = _ListingRepository.GetListingById(id);
            if (listing == null)
            {
                return HttpNotFound();
            }
            return View(listing);
        }

        // GET: Listings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Listings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ItemId,StartDate,EndDate,Title,CurrentPrice,QuantitySold,ListingStatus")] Listing listing)
        {
            if (ModelState.IsValid)
            {
                _ListingRepository.AddListing(listing);
                return RedirectToAction("Index");
            }

            return View(listing);
        }
        [HttpGet]
        public FileResult GetCsvReport()
        {

            var user = UserManager.FindById(User.Identity.GetUserId());
            string userId = user.Id;
            var listings = _InventoryBll.GetListingsByUser(-1, -1, userId).ToList();
            
            var myExport = new CsvExport();
            foreach (var listing in listings)
            {
                myExport.AddRow();
                myExport["Listing Id"] = listing.ItemId;
                myExport["Start Date"] = listing.StartDate;
                myExport["End Date"] = listing.EndDate;
                myExport["Listing Type"] = listing.Type;
                myExport["Title"] = listing.Title;                
                var qtySold = listing.CalculateQuantitySold();

                if (qtySold > 0)
                { 
                    myExport["Most Recent Sold Date"] = listing.Transactions.OrderByDescending(t=> t.CreatedDate).Select(t => t.CreatedDate).FirstOrDefault();
                }
                else
                {
                    myExport["Most Recent Sold Date"] = null;
                }
                myExport["Quantity Sold"] = qtySold;
                myExport["Average Sale Price"] = Math.Round(listing.AveragePrice,2);
                myExport["Total Fees"] = Math.Round(listing.TotalNetFees,2);
                myExport["Profit"] = Math.Round(listing.Profit,2);
                myExport["Status"] = listing.ListingStatus;
                myExport["Associated Item"] = listing.InventoryItem?.Description;
                myExport["Product Cost"] = listing.InventoryItem?.Cost;
            }
            var listingsCsv =  File(myExport.ExportToBytes(), "text/csv", "Listings_" + DateTime.Now + ".csv");

            return listingsCsv;

        }

        // POST: Listings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            _ListingRepository.DeleteListing(id);
            return RedirectToAction("Index");
        }

        public ActionResult UpdateFeesById(long id)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());            
            _ListingRepository.UpdateFeesById(id, user.Id);
            return RedirectToAction("Index");
        }

        public ActionResult UpdateListings()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            string userId = user.Id;
            //var _UserRepository = new UserRepository();
            //bool result = _UserRepository.TestUserToken(user.UserToken);

            _ListingBll.UpdateListings(userId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult AssociateInventoryItemToListing(string inventoryItemId, string listingItemId)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            var listingId = Convert.ToInt64(listingItemId);
            var listing = new Listing();
            int? invItemId;
            if (inventoryItemId == "-1")
            { 
                invItemId = null;
            }
            else
            {
                invItemId = int.Parse(inventoryItemId);
            }
            listing = _InventoryBll.AssociateInventoryItemToListing(listingId, invItemId, user.Id);
            return Json(new { profit = listing.Profit, cost = listing.InventoryItem?.Cost ?? 0 });
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}
