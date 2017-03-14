using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;

namespace TGVL.Models
{
    public class DealDetailsViewModel
    {
        public int Id { get; set; } //dealId
        public string Title { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public string UnitType { get; set; }

        public int Discount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal PriceSave { get; set; }

        public int Quantity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        [LessThanOrEqualTo("Quantity", ErrorMessage = "CustomerQuantity < Quantity")]
        public int CustomerQuantity { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public string DueDateCountdown { get; set; }

        public string Description { get; set; }

        public int Sold { get; set; }

        public SysProduct Product { get; set; }

        public string ProductImage { get; set; }

        public string ProductDetails { get; set; }

    }
}
