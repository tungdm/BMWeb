using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TGVL.Models
{
    public class LuceneSearch
    {
        public LuceneResult LuceneResult { get; set; }
       
        public string SearchString { get; set; }

    }

    public class LuceneResult
    {
        public IEnumerable<ProductSearchResult> SearchResult { get; set; }

        public string[] SuggestWords { get; set; }
    }

    public class ProductSearchResult
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        //public decimal UnitPrice { get; set; }

    }

    public class LuceneSelectedList
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
}