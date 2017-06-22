using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using StudentChoices.Models.Export;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace StudentChoices.Models
{
    public class AddGroups
    {
        public AddGroups()
        {
        }


        public int ClassGroupID { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public String DegreeCourse { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public byte Graduate { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public bool FullTimeStudies { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public byte Semester { get; set; }
        public String Speciality { get; set; }


}
}