using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Export
{
    public class ElectiveSubjectAndSpeciality
    {
        public ElectiveSubjectAndSpeciality()
        {

        }
        public ElectiveSubjectAndSpeciality(string name, List<string> studentno)
        {
            Name = name;
            StudentNo = studentno;
        }
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlElement("StudentNo")]
        public List<string> StudentNo { get; set; }
    }
    
}