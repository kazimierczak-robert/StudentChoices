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
    
    public partial class StudentChoices
    {
        public int StudentChoiceID { get; set; }
        public int StudentNo { get; set; }
        public int CategoryID { get; set; }
        public int ChoiceID { get; set; }
        public byte PreferenceNo { get; set; }
        public System.DateTime ChoiceDate { get; set; }
    
        public virtual Categories Categories { get; set; }
        public virtual ElectiveSubjectsAndSpecialities ElectiveSubjectsAndSpecialities { get; set; }
        public virtual Students Students { get; set; }
    }
}
