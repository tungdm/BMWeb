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
    
    public partial class UserProduct
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserProduct()
        {
            this.ContractProducts = new HashSet<ContractProduct>();
            this.Deals = new HashSet<Deal>();
        }
    
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int SysProductId { get; set; }
        public int Quantity { get; set; }
        public Nullable<decimal> PricePerUnit { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual SysProduct SysProduct { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ContractProduct> ContractProducts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Deal> Deals { get; set; }
    }
}
