using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StudentChoices.Models
{
    public class ChangePass
    {
        public ChangePass()
        {

        }

        //[Required(ErrorMessage = "To pole jest wymagane!")]
        [DataType(DataType.Password)]
        public string oldPassword { get; set; }


        //[Required(ErrorMessage = "To pole jest wymagane!")]
        [DataType(DataType.Password)]
        public string newPassword { get; set; }


        //[Required(ErrorMessage = "To pole jest wymagane!")]
        [DataType(DataType.Password)]
        public string newPassword2 { get; set; }
    }
}