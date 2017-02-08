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
    
    public partial class SysProduct
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SysProduct()
        {
            this.RequestProducts = new HashSet<RequestProduct>();
            this.SysProductAttributes = new HashSet<SysProductAttribute>();
            this.WarehouseProducts = new HashSet<WarehouseProduct>();
        }
    
        public int Id { get; set; }
        public int ManufacturerId { get; set; }
        public int SysCategoryId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<int> Flag { get; set; }
        public string Image { get; set; }
    
        public virtual Manufacturer Manufacturer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequestProduct> RequestProducts { get; set; }
        public virtual SysCategory SysCategory { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SysProductAttribute> SysProductAttributes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WarehouseProduct> WarehouseProducts { get; set; }
    }
}
