using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Export
{
    public class Admin
    {
        public Admin()
        {

        }
        public Admin(string login, string password, bool active, bool superadmin)
        {
            Login = login;
            Password = password;
            Active = active;
            SuperAdmin = superadmin;
        }
        [XmlAttribute("Login")]
        public string Login { get; set; }
        [XmlAttribute("Password")]
        public string Password { get; set; }
        [XmlAttribute("Active")]
        public bool Active { get; set; }
        [XmlAttribute("SuperAdmin")]
        public bool SuperAdmin { get; set; }
    }
    
}