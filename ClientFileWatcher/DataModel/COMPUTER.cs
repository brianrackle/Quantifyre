//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Client.DataModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class COMPUTER
    {
        public COMPUTER()
        {
            this.FILE = new HashSet<FILE>();
        }
    
        public long ID { get; set; }
        public string NAME { get; set; }
    
        public virtual ICollection<FILE> FILE { get; set; }
    }
}
