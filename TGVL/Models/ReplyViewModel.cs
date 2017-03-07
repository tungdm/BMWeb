using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class ReplyViewModel
    {
        //public ICollection<Product> Products { get; set; }

        [Required]
        public decimal Total { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

        [Required]
        public int Quantity { get; set; }

        public decimal Price { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày giao hàng")]
        [DateRange]
        public DateTime? DeliveryDate { get; set; }

        public ICollection<ReplyProductViewModel> ReplyProducts { get; set; }
    }

    public class WarehouseProductViewModel
    {
        public string Name { get; set; }

        public decimal UnitPrice { get; set; }

        public string Image { get; set; }

        public string Type { get; set; }

        public int Id { get; set; }
    }

    public class ReplyProductViewModel
    {
        public WarehouseProductViewModel Product { get; set; }

        public int? Quantity { get; set; }
    }

    //Thông tin sơ lược hiển thị ở trang request/details
    public class BriefReply
    {
        public int CustomerId { get; set; }

        public int SupplierId { get; set; }

        public string Fullname { get; set; }

        public string Avatar { get; set; }

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

        public decimal Total { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DeliveryDate { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }
    }

    public class ListReplies
    {
        public ICollection<BriefReply> Reply { get; set; }
    }

    public class ReplyDetails
    {
        public decimal Total { get; set; }
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveryDate { get; set; }

        public int Rank { get; set; }

        public string RequestType { get; set; } 
    }

    public class RepliedProduct
    {
        public string Name { get; set; }

        public decimal UnitPrice { get; set; }

        public string Image { get; set; }

        public string Type { get; set; }

        public int Id { get; set; }

        public int Quantity { get; set; }
    }
}