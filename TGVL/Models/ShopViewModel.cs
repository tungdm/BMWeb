using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.ObjectModel;

namespace TGVL.Models
{
    public class IndexShopViewModel
    {
        public string Username { get; set; }
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
}