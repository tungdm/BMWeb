using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class SearchResultViewModel
    {
        public SysProduct SysProduct { get; set; }
       
        public int NumOfShops { get; set; }

        public ICollection<Shop> ListShops { get; set; }

        public ICollection<SimiliarProduct> SimiliarProducts { get; set; }

        public string SearchString { get; set; }
    }

    public class SimiliarProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }

        public string UnitType { get; set; }

        public string Image { get; set; }

        public int NumShops { get; set; }
    }
}