using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.ObjectModel;
using System.Web.Mvc;

namespace TGVL.Models
{
    public class IndexShopViewModel
    {
        public string Username { get; set; }
    }

    public class HotShopViewModel
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }
        public float Rating { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
    }

  
    public class Shop
    {
        public int id { get; set; }
        public string name { get; set; }

        public string lat { get; set; }

        public string lng { get; set; }

        public string address { get; set; }

        public string address2 { get; set; }
        public string state { get; set; }

        public string city { get; set; }

        public string phone { get; set; }

        public string web { get; set; }

        public string hours1 { get; set; }
        public string hours2 { get; set; }
        public string hours3 { get; set; }

        public double rating { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = true)]
        public decimal price { get; set; }

        public int productId { get; set; }

    }


    public class AssignedWarehouseData
    {
        public int WarehouseId { get; set; }
        public string Address { get; set; }
        public bool Assigned { get; set; }
    }

    public class AddProductViewModel
    {
        //TODO: validate data
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int SysCategoryId { get; set; }
        public string ManufactureName { get; set; }
        public int ManufacturerId { get; set; }
        public string ProductImage { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int WarehouseId { get; set; }
        public int UnitTypeId { get; set; }
        public string UnitType { get; set; }

        //public IEnumerable<SysProduct> Products { get; set; }
        //public IEnumerable<SysCategory> Categories { get; set; }
        //public IEnumerable<Manufacturer> Manufacuters { get; set; }

        public IEnumerable<ProductAttribute> ProductAttributes { get; set; }



    }

    public class ResponeViewModel
    {
        public string Message { get; set; }
        public int WarehouseProductId { get; set; }
        public string Mode { get; set; }
    }


    public class UpdateDescription
    {
        public int Id { get; set; }

        [AllowHtml]
        public string Description { get; set; }
    }
}