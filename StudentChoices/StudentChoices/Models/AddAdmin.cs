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
    public class AddAdmin
    {
        public AddAdmin()
        {

        }

        public int AdminID { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Login { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Password { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public bool Active { get; set; }
        [Required(ErrorMessage = "To pole jest wymagane!")]
        public bool SuperAdmin { get; set; }
    }
}