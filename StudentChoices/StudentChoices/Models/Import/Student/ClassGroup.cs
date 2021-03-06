﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import.Student
{
    public class ClassGroup
    {
        public ClassGroup()
        {

        }
        public ClassGroup(string degreecourse, byte graduate, bool fulltimestudies, byte semester, string speciality, double averageGrade)
        {
            DegreeCourse = degreecourse;
            Graduate = graduate;
            FullTimeStudies = fulltimestudies;
            Semester = semester;
            Speciality = speciality;
            AverageGrade = averageGrade;
            ToRemove = false;
        }

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
        [XmlAttribute("AverageGrade")]
        public double AverageGrade { get; set; }
        public bool ToRemove { get; set; }
    }
}
