using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.ObjectModel;

namespace TGVL.Models
{
    public class IndexRequestViewModel
    {
        
    }

    public class CreateRequestViewModel
    {
        //TODO: validate data
        [Required]
        public string Title { get; set; }

        public int TypeOfHouse { get; set; }
        public string Administrative_area_level_1 { get; set; } //tỉnh, thành phố
        public string Formatted_address { get; set; } //địa chỉ đầy đủ (delivery address)

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày nhận hàng")]
        public DateTime? ReceivingDate { get; set; }

        public int PaymentType { get; set; }
        public string Description { get; set; }
        
        public int TimeRange { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        public string RequestType { get; set; }

        public IEnumerable<SysProduct> RequestProducts { get; set; }
    }

    public class RequestProductViewModel
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
        public ICollection<int> ListQuantity { get; set; }

        public string UnitType { get; set; }

        public ICollection<SysProduct> Products { get; set; }

        public ICollection<SysProduct> SelectedProduct { get; set; }

        public IEnumerable<ProductAttribute> ProductAttributes { get; set; }

        public string Message { get; set; }

        
    }

}