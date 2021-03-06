﻿using EbaySalesTracker.Models;
using System.Collections.Generic;

namespace EbaySalesTracker.Bll
{

    public interface IInventoryBll
    {
        InventoryItem GetInventoryItemById(int inventoryItemId, string userId);
        IEnumerable<InventoryItem> GetInventoryItemsByUser(string userId);
        InventoryItem GetBestSellingItem(string userId);
        InventoryItem GetHighestAverageProfitItem(string userId);
        double GetProfitByMonth(int year, int month, string userId);
        double GetSalesLastSevenDays(string userId);
        double GetSalesByMonth(int year, int month, string userId);
        double GetSalesByYear(int year, string userId);
        object GetListingDataByInventoryItem(int inventoryItemId, string userId);
        Listing AssociateInventoryItemToListing(long listingId, int? inventoryItemId, string userId);
        IEnumerable<Listing> GetListingsByUser(int top, int skip,string userId);

        //should probably refactor listings controller so that I don't need this calculate method exposed 
        double CalculateProfitPerListing(Listing listing, string userId);
        double GetProfitLastSevenDays(string userId);
        double GetProfitByYear(int year, string userId);
        double GetFeesByYear(int year, string userId);
        double GetFeesLastSevenDays(string userId);
        double GetFeesByMonth(int year, int month, string userId);
        
    }
}
