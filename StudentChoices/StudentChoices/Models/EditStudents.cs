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
    public class EditStudents
    {
        public EditStudents()
        {

        }

        [Editable(false)]
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public int StudentNo { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Login { get; set; }

        //[Required(ErrorMessage = "To pole jest wymagane!")]
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