using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hiale.Podbook.Feed
{
    public class Channel
    {
        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }

        [XmlElement("item")]
        public List<Episode> Episodes { get; set; }

        public Channel()
        {
            Episodes = new List<Episode>();
            Description = string.Empty;
            Link = string.Empty;
        }
    }
}
