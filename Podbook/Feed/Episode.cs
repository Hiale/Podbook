using System;
using System.Xml.Serialization;

// ReSharper disable ValueParameterNotUsed

namespace Hiale.Podbook.Feed
{
    public class Episode
    {
        [XmlElement("guid")]
        public string Guid
        {
            get { return Url; }
            set { }
        }

        [XmlElement("link")]
        public string Url
        {
            get => Enclosure.Url;
            set => Enclosure.Url = value;
        }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlIgnore]
        public DateTime Timestamp { get; set; }

        [XmlElement("pubDate")]
        public string PubDate
        {
            get { return Timestamp.ToString("r"); }
            set { }
        }

        [XmlIgnore]
        public long Length
        {
            get => Enclosure.Length;
            set => Enclosure.Length = value;
        }

        [XmlElement("enclosure")]
        public Enclosure Enclosure { get; set; }

        [XmlIgnore]
        public uint Track { get; set; }

        private Episode()
        {
            Enclosure = new Enclosure {Type = "audio/mpeg"};
        }

        public Episode(string url) : this()
        {
            Url = url;
            Description = string.Empty;
        }
    }
}
