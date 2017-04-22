using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TGVL.Models;

namespace TGVL.Models
{
    public class OrderViewModel
    {
        public ShoppingCart ShoppingCart {get; set;}

        public int CustomerId { get; set; }

        [Required(ErrorMessage ="Tên khách hàng không được bỏ trống")]
        public string CustomerFullName { get; set; }

        [Required(ErrorMessage ="Địa chỉ không được bỏ trống")]
        public string Address { get; set; }

        [Required(ErrorMessage ="Số điện thoại không được bỏ trống")]
        [RegularExpression(@"^(09.|011.|012.|013.|014.|015.|016.|017.|018.|019.|08.)\d{7}$", ErrorMessage = "Not a valid Phone number")]
        public string PhoneNumber { get; set; }

        public IEnumerable<Payment> AllTypeOfPayments { get; set; }

        [Required(ErrorMessage ="Hình thức thanh toán không được bỏ trống")]
        public int PaymentType { get; set; }

        public string Description { get; set; }

        public bool IsRequestOrder { get; set; }

        public Reply Reply { get; set; }

    }

    public class MyOrder
    {
        public int Id { get; set; }

        public string SupplierName { get; set;}

        public DateTime DeliveryDate { get; set; }

        public string Status { get; set; }

    }

    public class OrderMail
    {
        public string Email { get; set; }

        public string FullName { get; set; }

        public string Administrative_area_level_1 { get; set; }

        public string Address { get; set; }

        public string PhoneNumber { get; set; }

        public string Code { get; set; }
        
        public string Payment { get; set; }

        public decimal Total { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string CallbackURL { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public class ReviewViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage ="Giá bán không được để trống")]
        public int PriceGrade { get; set; }
        public IEnumerable<Grade> PriceGrades { get; set; }

        [Required(ErrorMessage = "Chất lượng sản phẩm không được để trống")]
        public int QualityGrade { get; set; }
        public IEnumerable<Grade> QualityGrades { get; set; }

        [Required(ErrorMessage = "Mức độ phục vụ không được để trống")]
        public int ServiceGrade { get; set; }
        public IEnumerable<Grade> ServiceGrades { get; set; }

        public string Comment { get; set; }

    }

    
}