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
    
    public partial class Students
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Students()
        {
            this.FinalChoices = new HashSet<FinalChoices>();
            this.StudentChoices = new HashSet<StudentChoices>();
            this.StudentsAndClassGroups = new HashSet<StudentsAndClassGroups>();
        }
    
        public int StudentNo { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public byte TriesNo { get; set; }
        public Nullable<System.DateTime> LastEdit { get; set; }
        public Nullable<int> LastEditedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public int CreatedBy { get; set; }
    
        public virtual Admins Admins { get; set; }
        public virtual Admins Admins1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FinalChoices> FinalChoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentChoices> StudentChoices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentsAndClassGroups> StudentsAndClassGroups { get; set; }
    }
}
