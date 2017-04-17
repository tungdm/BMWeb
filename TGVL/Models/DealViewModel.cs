using Foolproof;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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

        [Required(ErrorMessage = "Số lượng không được để trống")]
        public int CustomerQuantity { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public string DueDateCountdown { get; set; }

        public string Description { get; set; }

        public string ShortDescription { get; set; }

        public int NumBuyer { get; set; }

        public SysProduct Product { get; set; }

        public string ProductImage { get; set; }

        public string ProductDetails { get; set; }

        public bool Expired { get; set; }

        public ICollection<SimilarDeal> SimilarDeals { get; set; }
        public ICollection<SimilarDeal> SameSuppliers { get; set; }
    }

    public class SimilarDeal
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public int Discount { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal PriceSave { get; set; }

        public int NumBuyer { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public string DueDateCountdown { get; set; }

    }

    public class DealBriefViewModel
    {
        public int Id { get; set; } //dealId

        public string Title { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal UnitPrice { get; set; }

        public int Discount { get; set; }

        public int NumBuyer { get; set; }

        public string Image { get; set; }

        [DisplayFormat(DataFormatString = "{0:C0}")]
        public decimal SavePrice { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; }

        public string DueDateCountdown { get; set; }

    }
    public class HydridViewModel
    {
        public ICollection<DealBriefViewModel> Hotdeal { get; set; }

        public ICollection<DealBriefViewModel> Newdeal { get; set; }

        public ICollection<HotShopViewModel> Hotshop { get; set; }

        public ICollection<RequestFloorModel> NewRequest { get; set; }
    }
}
