using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import.Student
{
    public class Student
    {
        public Student()
        {

        }
        public Student(int studentNo, string login, string password, string name, string surname, List<ClassGroup> classGroup)
        {
            StudentNo = studentNo;
            Login = login;
            Password = password;
            Name = name;
            Surname = surname;
            ClassGroup = classGroup;
            ToRemove = false;
        }

        [XmlAttribute("StudentNo")]
        public int StudentNo { get; set; }
        [XmlAttribute("Login")]
        public string Login { get; set; }
        [XmlAttribute("Password")]
        public string Password { get; set; }
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Surname")]
        public string Surname { get; set; }
        [XmlElement("ClassGroup")]
        public List<ClassGroup> ClassGroup { get; set; }
        public bool ToRemove { get; set; }
    }
}
