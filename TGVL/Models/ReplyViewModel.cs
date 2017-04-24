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
     
        [Required(ErrorMessage ="Tổng cộng không được để trống")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = true)]
        public decimal Total { get; set; }

        //[Required(ErrorMessage ="Giá thầu không được để trống")]
        public string BidPrice { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

        [Required(ErrorMessage ="Số lượng không được để trống")]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ngày giao hàng không được để trống")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày giao hàng")]
        [DateRange]
        public DateTime? DeliveryDate { get; set; }

        [Required(ErrorMessage ="Phí vận chuyển không được để trống")]
        [Display(Name = "Phí vận chuyển")]
        [Range(0, int.MaxValue, ErrorMessage = "Xin hãy nhập giá trị lớn hơn 0")]
        public int ShippingFee { get; set; }

        [Display(Name = "Giảm giá(%)")]
        [Range(0, 100, ErrorMessage = "Xin hãy nhập trong khoảng từ 1 đến 100")]
        public int Discount { get; set; }

        //public ICollection<ReplyProductViewModel> ReplyProducts { get; set; }

        public IList<ReplyProductViewModel> ReplyProductsTest { get; set; }

        public int Flag { get; set; }

        public int MinDateDeliveryRange { get; set; }

        public int MaxLengthInputNumberBig { get; set; }

        public int MaxYearInput { get; set; }
        public bool Policies { get; set; }

       
        public DateTime DueDate { get; set; }
    }

    public class AutobidViewModel
    {
        public int ReplyId { get; set; }

        public string CurentPrice { get; set; }

        //[Required]
        public string MinimumPrice { get; set; }

        //[Required]
        public string Deduction { get; set; }

        public string Type { get; set; } //tao moi hoac update (huy)
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
        [Range(1, int.MaxValue)]
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

        [Required(ErrorMessage = "Ngày giao hàng không được để trống")]
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

        public int RequestId { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

        public string BidPrice { get; set; }

        //[DisplayFormat(DataFormatString = "{0:C0}")]
        //[LessThanOrEqualTo("Total", ErrorMessage="Giá mới phải bằng hoặc nhỏ hơn giá cũ")]
        //public decimal NewTotal { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage ="Phí vận chuyển không được để trống")]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        [Range(0, int.MaxValue, ErrorMessage = "Phí vận chuyển phải lớn hơn hoặc bằng 0")]
        public int ShippingFee { get; set; }

        [Required(ErrorMessage ="Giảm giá không được để trống")]
        [Range(0, 100, ErrorMessage = "Xin hãy nhập trong khoảng từ 1 đến 100")]
        public int Discount { get; set; }

        public int Flag { get; set; }

        [Required(ErrorMessage = "Ngày giao hàng không được để trống")]
        [DataType(DataType.Date)]
        [DateRange]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; }

        public int Rank { get; set; }

        public string RequestType { get; set; }

        public IList<RepliedProduct> ReplyProducts { get; set; }

        public int MinDateDeliveryRange { get; set; }

        public DateTime DueDate { get; set; }

        public int MaxYearInput { get; set; }

        public string SupplierName { get; set; }

        public int SupplierId { get; set; }

        [DisplayFormat(DataFormatString = "{0:####-###-###}")]
        public string PhoneNumber { get; set; }

        public List<string> WarehouseAddress { get; set; }

        public double Rating { get; set; }

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

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Xin hãy nhập giá trị lớn hơn 0")]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }
    }
}