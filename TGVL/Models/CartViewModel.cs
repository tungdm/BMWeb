using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class CartViewModel
    {
        public ICollection<CartDetails> CartDetails { get; set; }

    }

    public class CartDetails
    {
        public int DealId { get; set; }

        public int ProductId { get; set; }

        public string Type { get; set; }
        public int Quantity { get; set; }
    }

    public class ShoppingCart
    {
        public IList<ShoppingCartProducts> ShoppingCartProducts { get; set; }
        public int CustomerId { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

    }

    public class ShoppingCartProducts
    {
        public int ProductId { get; set; } //cho t/h mua rieng tung san pham

        public int DealId { get; set; } //cho t/h mua deal

        public string Type { get; set; } //Type=normal/deal

        public string Image { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; } //Gia cuoi cung luc mua (sau khi tinh tong cac chi phi)

        public string UnitType { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal MiniTotal { get; set; } //UnitPrice * Quantity
    }
}