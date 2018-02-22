using System.Xml.Serialization;

namespace Hiale.Podbook.Feed
{
    [XmlRoot("rss")]
    public class Rss
    {
        [XmlAttribute("version")]
        public string Version
        {
            get { return "2.0"; }
            // ReSharper disable once ValueParameterNotUsed
            set { }
        }

        [XmlElement("channel")]
        public Channel Channel { get; set; }
    }
}
