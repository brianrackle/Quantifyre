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
    
    public partial class USER_GROUP
    {
        public USER_GROUP()
        {
            this.USER = new HashSet<USER>();
            this.EXTENSION = new HashSet<EXTENSION>();
            this.PROCESS = new HashSet<PROCESS>();
        }
    
        public long ID { get; set; }
        public string NAME { get; set; }
        public string FILTER { get; set; }
        public long GROUP_SETTING_ID { get; set; }
    
        public virtual GROUP_SETTING GROUP_SETTING { get; set; }
        public virtual ICollection<USER> USER { get; set; }
        public virtual ICollection<EXTENSION> EXTENSION { get; set; }
        public virtual ICollection<PROCESS> PROCESS { get; set; }
    }
}
