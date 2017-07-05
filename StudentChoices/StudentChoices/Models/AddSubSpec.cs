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
    public class AddSubSpec
    {
        public AddSubSpec()
        {

        }

        //[Required(ErrorMessage = "To pole jest wymagane!")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Name { get; set; }

        public string Information { get; set; }

        //[Required(ErrorMessage = "To pole jest wymagane!")]
        public int ElectiveSubjectAndSpecialityID { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public short? UpperLimit { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public short? LowerLimit { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public int ClassGroupID { get; set; }
    }
}