//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TGVL
{
    using System;
    using System.Collections.Generic;
    
    public partial class RequestProduct
    {
        public int Id { get; set; }
        public Nullable<int> RequestId { get; set; }
        public int SysProductId { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual Request Request { get; set; }
        public virtual SysProduct SysProduct { get; set; }
    }
}
