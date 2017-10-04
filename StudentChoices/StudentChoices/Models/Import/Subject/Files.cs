using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace StudentChoices.Models.Import.Subject
{
    public class Files
    {
        public Files()
        {

        }
        public Files(string filename, string path)
        {
            Filename = filename;
            Path = path;
            ToRemove = false;
        }

        [XmlAttribute("Filename")]
        public string Filename { get; set; }
        [XmlAttribute("Path")]
        public string Path { get; set; }
        public bool ToRemove { get; set; }
    }
}