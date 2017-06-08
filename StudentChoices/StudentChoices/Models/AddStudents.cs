using StudentChoices.Models.Export;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models
{
    public class AddStudents
    {
        public AddStudents()
        {

        }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public int StudentNo { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Login { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public double AverageGrade { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public int ClassGroupID { get; set; }
        
    }
}