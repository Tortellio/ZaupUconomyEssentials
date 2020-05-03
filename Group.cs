using System.Xml.Serialization;

namespace ZaupUconomyEssentials
{
    [XmlRoot("UconomyEConfiguration")]
    public class Group
    {
        [XmlAttribute] public string GroupID { get; set; }
        [XmlAttribute] public decimal Salary { get; set; }
    }

    public class PremiumGroup
    {
        [XmlAttribute] public string GroupID { get; set; }
        [XmlAttribute] public decimal Salary { get; set; }
    }
}