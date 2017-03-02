using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class ReplyViewModel
    {
        //public ICollection<Product> Products { get; set; }

        public decimal Total { get; set; }

        public string Description { get; set; }

        public string Message { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

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
        public string Fullname { get; set; }

        public string Avatar { get; set; }

        public decimal Total { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }
    }

    public class ListReplies
    {
        public ICollection<BriefReply> Reply { get; set; }
    }
}