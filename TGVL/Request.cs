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
    
    public partial class Request
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Request()
        {
            this.Replies = new HashSet<Reply>();
            this.RequestDescriptions = new HashSet<RequestDescription>();
            this.RequestProducts = new HashSet<RequestProduct>();
        }
    
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<decimal> CustomerPrice { get; set; }
        public string DeliveryAddress { get; set; }
        public Nullable<System.DateTime> DeliveryDate { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.DateTime DueDate { get; set; }
        public string PaymentType { get; set; }
        public int RequestTypeId { get; set; }
        public string Title { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual BiddingFloor BiddingFloor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reply> Replies { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequestDescription> RequestDescriptions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequestProduct> RequestProducts { get; set; }
        public virtual User User { get; set; }
        public virtual RequestType RequestType { get; set; }
        public virtual User User1 { get; set; }
        public virtual Review Review { get; set; }
    }
}
