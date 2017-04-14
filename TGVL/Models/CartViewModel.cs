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

        public string Type { get; set; } //deal hoac mua ngay
        public int Quantity { get; set; }
    }

    public class ShoppingCart
    {
        public IList<ShoppingCartProducts> ShoppingCartProducts { get; set; }
        public int CustomerId { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal Total { get; set; }

        public int MaxLengthInputNumberSmall { get; set; }
    }

    public class ShoppingCartProducts
    {
        public int ProductId { get; set; } //cho t/h mua rieng tung san pham

        public int SysProductId { get; set; }

        public int DealId { get; set; } //cho t/h mua deal

        public int SupplierId { get; set; }

        public string SupplierName { get; set; }

        public string Type { get; set; } //Type = normal/deal

        public string Image { get; set; }

        public string ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Xin hãy nhập giá trị lớn hơn 0")]
        public int Quantity { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; } //Gia cuoi cung luc mua (sau khi tinh tong cac chi phi)

        public string UnitType { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal MiniTotal { get; set; } //UnitPrice * Quantity
    }

    public class MuaNgayViewModel
    {
        public int Id { get; set; }


        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; } 

        public string UnitType { get; set; }

        public string ProductName { get; set; }

        [Required(ErrorMessage = "Số lượng không được bỏ trống")]
        
        public int Quantity { get; set; }
    }
}