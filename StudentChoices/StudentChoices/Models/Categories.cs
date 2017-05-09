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
    
    public partial class Categories
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Categories()
        {
            this.ElectiveSubjectsAndSpecialities = new HashSet<ElectiveSubjectsAndSpecialities>();
            this.FinalChoices = new HashSet<FinalChoices>();
            this.StudentChoices = new HashSet<StudentChoices>();
        }
    
        public int CategoryID { get; set; }
        public int ClassGroupID { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public int MaxNoChoices { get; set; }
        public Nullable<System.DateTime> LastEdit { get; set; }
        public Nullable<int> LastEditedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual Admins Admins { get; set; }
        public virtual Admins Admins1 { get; set; }
        public virtual ClassGroups ClassGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ElectiveSubjectsAndSpecialities> ElectiveSubjectsAndSpecialities { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FinalChoices> FinalChoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentChoices> StudentChoices { get; set; }
    }
}