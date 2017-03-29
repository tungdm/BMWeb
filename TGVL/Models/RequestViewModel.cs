using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Web.Mvc;

namespace TGVL.Models
{
    public class IndexRequestViewModel
    {
        
    }
    public class RequestFloorModel
    {
        public string Title { get; set; }

        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [DateRange]
        public DateTime? ReceivingDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [DateRange]
        public DateTime? DueDate { get; set; }

        public string Descriptions { get; set; }

        public string DeliveryAddress { get; set; }

        public string UserName { get; set; }

        public string Avatar {get;set;}

    }

    public class CreateRequestViewModel
    {
        //TODO: validate data
        [Required(ErrorMessage ="Tiêu đề không được để trống")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; }

        [Required(ErrorMessage ="Loại nhà không được để trống")]
        [Display(Name = "Loại nhà")]
        public int TypeOfHouse { get; set; }

        public IEnumerable<House> AllTypeOfHouses { get; set; }

        public string Administrative_area_level_1 { get; set; } //tỉnh, thành phố
        public string Formatted_address { get; set; } //địa chỉ đầy đủ (delivery address)

        [Required(ErrorMessage = "Ngày nhận hàng không được để trống")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày nhận hàng")]
        [DateRange]
        public DateTime? ReceivingDate { get; set; }

        [Required(ErrorMessage ="Địa chỉ nhận hàng không được để trống")]
        public string ReceivingAddress { get; set; }

        [Required(ErrorMessage ="Hình thức thanh toán không được để trống")]
        [Display(Name = "Hình thức thanh toán")]
        public int PaymentType { get; set; }

        public IEnumerable<Payment> AllTypeOfPayments { get; set; }

        [Display(Name = "Miêu tả chi tiết")]
        [AllowHtml]
        public string Description { get; set; }

        [Required(ErrorMessage = "Thời hạn yêu cầu không được để trống")]
        //[Range(1, 30, ErrorMessage = "Thời hạn yêu cầu từ 1 đến 30 ngày")]
        [Display(Name = "Thời hạn yêu cầu")]
        //[MaxLength(20, ErrorMessage = "ABC")]
        public int TimeRange { get; set; }

        [Display(Name = "Người yêu cầu")]
        public string CustomerName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }


        public string RequestType { get; set; }

        //public IEnumerable<SysProduct> RequestProducts { get; set; }

        public IList<RequestedProductWithQuantity> RequestProducts { get; set; }

        public string Flag { get; set; }

        //Validate
        public int MinNumSeletedProduct { get; set; }
        public int MaxLengthInputNumberBig { get; set; }
        public int MaxLengthInputNumberSmall { get; set; }
        public int MinBidPrice { get; set; }
        public int MinLengthInputText { get; set; }
        public int MaxLengthInputTextSmall { get; set; }
        public int MinTimeRange { get; set; }
        public int MaxTimeRange { get; set; }

        public int MinDateDeliveryRange { get; set; }
    }

    public class RequestedProduct
    {
        public string Name { get; set; } //Product name

        public string ManufactureName { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public string Image { get; set; }

        public string Type { get; set; }

        public int Id { get; set; } //ProductId
             
        
    }

    public class RequestedProductWithQuantity
    {
        public RequestedProduct RequestedProduct { get; set; }

        public int Quantity { get; set; }
    }


    public class RequestProductViewModel
    {
        //TODO: validate data
        public int ProductId { get; set; }

        [Display(Name = "Sản phẩm")]
        public string ProductName { get; set; }

        [Display(Name = "Danh mục")]
        public string CategoryName { get; set; }


        public int SysCategoryId { get; set; }
        [Display(Name = "Nhà sản xuất")]
        public string ManufactureName { get; set; }
        public int ManufacturerId { get; set; }

        [Display(Name = "Ảnh minh họa")]
        public string ProductImage { get; set; }

        [Display(Name = "Giá")]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }


        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }
        public ICollection<int> ListQuantity { get; set; }

        [Display(Name = "Đơn vị tính")]
        public string UnitType { get; set; }

        public ICollection<SysProduct> Products { get; set; }

        public ICollection<ProductSearchResult> SelectedProduct { get; set; }

        public IEnumerable<ProductAttribute> ProductAttributes { get; set; }

        public string Message { get; set; }

        public string Flag { get; set; }

        public ICollection<ProductSearchResult> SearchResult { get; set; }
        public string[] SuggestWords { get; set; }

        
    }

}