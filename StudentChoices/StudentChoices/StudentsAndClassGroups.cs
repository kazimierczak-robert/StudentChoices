//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentChoices
{
    using System;
    using System.Collections.Generic;
    
    public partial class StudentsAndClassGroups
    {
        public int StudentNo { get; set; }
        public int ClassGroupID { get; set; }
        public Nullable<System.DateTime> LastEdit { get; set; }
        public Nullable<int> LastEditedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual Admins Admins { get; set; }
        public virtual Admins Admins1 { get; set; }
        public virtual ClassGroups ClassGroups { get; set; }
        public virtual Students Students { get; set; }
    }
}
