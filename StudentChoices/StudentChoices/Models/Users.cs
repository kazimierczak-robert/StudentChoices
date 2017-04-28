namespace StudentChoices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Users
    {
        public Users()
        {

        }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        public string Login { get; set; }

        [Required(ErrorMessage = "To pole jest wymagane!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
