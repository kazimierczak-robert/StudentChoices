using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import.Subject
{
    public class ElectiveSubjectAndSpeciality
    {
        public ElectiveSubjectAndSpeciality()
        {

        }
        public ElectiveSubjectAndSpeciality(string name, string information, string upperLimit, string lowerLimit, List<Files> files)
        {
            Name = name;
            Information = information;
            UpperLimit = upperLimit;
            LowerLimit = lowerLimit;
            Files = files;
            ToRemove = false;
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Information")]
        public string Information { get; set; }
        [XmlAttribute("UpperLimit")]
        public string UpperLimit { get; set; }
        [XmlAttribute("LowerLimit")]
        public string LowerLimit { get; set; }
        [XmlElement("Files")]
        public List<Files> Files { get; set; }
        public bool ToRemove { get; set; }
    }
}