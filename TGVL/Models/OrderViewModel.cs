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

        [Required]
        public string CustomerFullName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [RegularExpression(@"^(09.|011.|012.|013.|014.|015.|016.|017.|018.|019.|08.)\d{7}$", ErrorMessage = "Not a valid Phone number")]
        public string PhoneNumber { get; set; }

        public IEnumerable<Payment> AllTypeOfPayments { get; set; }

        [Required]
        public int PaymentType { get; set; }

        public string Description { get; set; }

        public string OrderType { get; set; }

        public int RequestId { get; set; }

        public int ReplyId { get; set; }
    }
}