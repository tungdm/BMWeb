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
    
    public partial class Deal
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Deal()
        {
            this.Orders = new HashSet<Order>();
        }
    
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseProductId { get; set; }
        public string Title { get; set; }
        public decimal UnitPrice { get; set; }
        public int Discount { get; set; }
        public int Quantity { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual WarehouseProduct WarehouseProduct { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Orders { get; set; }
    }
}
