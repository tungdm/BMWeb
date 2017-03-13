using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class ReplyViewModel
    {
     
        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

        [Required]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày giao hàng")]
        [DateRange(ErrorMessage = "Ngày giao hàng phải từ ngày hôm nay về sau")]
        public DateTime? DeliveryDate { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a value >= 0")]
        public int ShippingFee { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Please enter a value >= 0 and <= 100")]
        public int Discount { get; set; }

        //public ICollection<ReplyProductViewModel> ReplyProducts { get; set; }

        public IList<ReplyProductViewModel> ReplyProductsTest { get; set; }

        public int Flag { get; set; }
    }

    public class WarehouseProductViewModel
    {
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public string Image { get; set; }

        public string Type { get; set; }

        public int Id { get; set; }
    }

    public class ReplyProductViewModel
    {
        public WarehouseProductViewModel Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value > 0")]
        public int? Quantity { get; set; }
    }

    //Thông tin sơ lược hiển thị ở trang request/details
    public class BriefReply
    {
        public int CustomerId { get; set; }

        public int SupplierId { get; set; }

        public string Fullname { get; set; }

        public string Avatar { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public int Id { get; set; }
    }

    public class BriefBidReply
    {
        public int Rank { get; set; }

        public string Fullname { get; set; }

        public string Avatar { get; set; }

        public string Address { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }

        public int SupplierId { get; set; }
        public int CustomerId { get; set; }
    }

    public class ListReplies
    {
        public ICollection<BriefReply> Reply { get; set; }
    }

    public class ReplyDetails
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

        //[DisplayFormat(DataFormatString = "{0:C0}")]
        //[LessThanOrEqualTo("Total", ErrorMessage="Giá mới phải bằng hoặc nhỏ hơn giá cũ")]
        //public decimal NewTotal { get; set; }
        public string Description { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter a value >= 0")]
        public int ShippingFee { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Please enter a value >= 0 and <= 100")]
        public int Discount { get; set; }

        public int Flag { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DateRange(ErrorMessage = "Ngày giao hàng phải từ ngày hôm nay về sau")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; }

        public int Rank { get; set; }

        public string RequestType { get; set; }

        public IList<RepliedProduct> ReplyProducts { get; set; }
    }

    public class RepliedProduct
    {
        public string Name { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public string Image { get; set; }

        public string Type { get; set; }

        public int Id { get; set; } //ProductId

        public int ReplyProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }
    }
}