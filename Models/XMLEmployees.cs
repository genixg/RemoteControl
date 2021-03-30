using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RemoteControl.Models
{


    [Serializable()]
    [XmlRoot("Данные")]
    public class XMLDocumentEmployees
    {
        [XmlArray("Подразделения")]
        [XmlArrayItem("Элемент", typeof(XMLDepartment))]
        public List<XMLDepartment> depts { get; set; }


        [XmlArray("Сотрудники")]
        [XmlArrayItem("Элемент")]
        public List<XMLEmployee> employees { get; set; }
    }

    [Serializable()]
    public class XMLDepartment
    {
        [System.Xml.Serialization.XmlElementAttribute("Наименование")]
        public string Name { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("GUID")]
        public string GUID { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Родитель")]
        public string Parent { get; set; }
    }

    [Serializable()]
    public class XMLEmployee
    {
        [System.Xml.Serialization.XmlElementAttribute("Наименование")]
        public string Name { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("GUID")]
        public string GUID { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Должность")]
        public string Role { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Подразделение")]
        public string Department { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("СлужебныйТелефон")]
        public string WorkPhone { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("ВнутреннийТелефон")]
        public string InnerPhone { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("МобильныйТелефон")]
        public string MobilePhone { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("ГазовыйТелефон")]
        public string GazPhone { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Email")]
        public string Email { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Город")]
        public string City { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Местоположение")]
        public string Address { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("ДатаРождения")]
        public string Birthsday { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("Состояние")]
        public string Status { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("СостояниеДействуетДо")]
        public string StatusTill { get; set; }
    }
}

