using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import.Subject
{
    public class Category
    {
        public Category()
        {

        }
        public Category(string name, string information, int maxNoChoices, List<ElectiveSubjectAndSpeciality> electiveSubjectAndSpeciality)
        {
            Name = name;
            Information = information;
            MaxNoChoices = maxNoChoices;
            ElectiveSubjectAndSpeciality = electiveSubjectAndSpeciality;
            ToRemove = false;
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Information")]
        public string Information { get; set; }
        [XmlAttribute("MaxNoChoices")]
        public int MaxNoChoices { get; set; }
        [XmlElement("ElectiveSubjectAndSpeciality")]
        public List<ElectiveSubjectAndSpeciality> ElectiveSubjectAndSpeciality { get; set; }
        public bool ToRemove { get; set; }
    }
}