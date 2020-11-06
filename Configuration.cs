using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AIMP_RYMSetListeningTo
{
    [XmlRoot(ElementName = "Configuration")]
    public class Configuration
    {
        [XmlElement(ElementName = "AuthToken")]
        public string AuthToken { get; set; }

        [XmlElement(ElementName = "IntervalBetweenRequests")]
        public int IntervalBetweenRequests { get; set; }

        public override string ToString()
        {
            return AuthToken + "\n" + IntervalBetweenRequests;
        }
    }
}
