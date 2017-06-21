using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WindowsTimeService
{
    [XmlRoot("Paramlist")]
    public class Paramlist
    {
        [XmlElement("Param")]
        public List<Param> DataSource { get; set; }
    }

    [XmlType("Param")]
    public class Param
    {
        [XmlAttribute("hour")]
        public string Hour { get; set; }

        [XmlAttribute("minute")]
        public string Minute { get; set; }

        [XmlAttribute("url")]
        public string Url { get; set; }
    }
}
