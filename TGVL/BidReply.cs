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
    
    public partial class BidReply
    {
        public int ReplyId { get; set; }
        public int Rank { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> Flag { get; set; }
    
        public virtual Reply Reply { get; set; }
    }
}