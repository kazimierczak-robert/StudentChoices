//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StudentChoices.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Files
    {
        public int FileID { get; set; }
        public string Filename { get; set; }
        public int ElectiveSubjectAndSpecialityID { get; set; }
        public string Path { get; set; }
        public Nullable<System.DateTime> LastEdit { get; set; }
        public Nullable<int> LastEditedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual Admins Admins { get; set; }
        public virtual Admins Admins1 { get; set; }
        public virtual ElectiveSubjectsAndSpecialities ElectiveSubjectsAndSpecialities { get; set; }
    }
}