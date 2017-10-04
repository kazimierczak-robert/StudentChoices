using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import
{
    public class Admin
    {
        public Admin()
        {

        }
        public Admin(string login, string password, bool superAdmin)
        {
            Login = login;
            Password = password;
            ToRemove = false;
        }
        [XmlAttribute("Login")]
        public string Login { get; set; }
        [XmlAttribute("Password")]
        public string Password { get; set; }
        public bool ToRemove { get; set; }
    }

}