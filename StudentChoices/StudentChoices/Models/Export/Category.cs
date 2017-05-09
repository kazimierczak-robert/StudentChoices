using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Export
{
    public class Category
    {
        public Category()
        {

        }
        public Category(string name, string degreecourse, byte graduate, bool fulltimestudies, byte semester, string speciality, List<ElectiveSubjectAndSpeciality> electivedubjectsndspeciality)
        {
            Name = name;
            DegreeCourse = degreecourse;
            Graduate = graduate;
            FullTimeStudies = fulltimestudies;
            Semester = semester;
            if(speciality=="-")
            {
                Speciality = "";
            }
            else
            { 
                Speciality = speciality;
            }
            ElectiveSubjectAndSpeciality = electivedubjectsndspeciality;
        }

        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("DegreeCourse")]
        public string DegreeCourse { get; set; }
        [XmlAttribute("Graduate")]
        public byte Graduate { get; set; }
        [XmlAttribute("FullTimeStudies")]
        public bool FullTimeStudies { get; set; }
        [XmlAttribute("Semester")]
        public byte Semester { get; set; }
        [XmlAttribute("Speciality")]
        public string Speciality { get; set; }
        [XmlElement("ElectiveSubjectAndSpeciality")]
        public List<ElectiveSubjectAndSpeciality> ElectiveSubjectAndSpeciality { get; set; }
    }
}