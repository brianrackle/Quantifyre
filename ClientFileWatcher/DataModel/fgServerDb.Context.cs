﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class fgServerDb : DbContext
    {
        public fgServerDb()
            : base("name=fgServerDb")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<COMPUTER> COMPUTER { get; set; }
        public DbSet<DOMAIN> DOMAIN { get; set; }
        public DbSet<EXTENSION> EXTENSION { get; set; }
        public DbSet<FILE> FILE { get; set; }
        public DbSet<FILE_EVENT> FILE_EVENT { get; set; }
        public DbSet<GROUP_SETTING> GROUP_SETTING { get; set; }
        public DbSet<PROCESS> PROCESS { get; set; }
        public DbSet<USER> USER { get; set; }
        public DbSet<USER_GROUP> USER_GROUP { get; set; }
    }
}
