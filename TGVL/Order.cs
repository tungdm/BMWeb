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
    
    public partial class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int DealId { get; set; }
        public int Quantity { get; set; }
        public string PaymentType { get; set; }
        public decimal Total { get; set; }
        public string Code { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual Deal Deal { get; set; }
        public virtual User User { get; set; }
    }
}
