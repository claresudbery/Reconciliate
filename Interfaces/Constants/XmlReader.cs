using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Interfaces.Constants
{
    public class MyXmlReader
    {
        public string Xml_file_path => ReadXml(
            FilePathConsts.ConfigPathProperty, 
            Path.Combine(FilePathConsts.ConfigFilePath, FilePathConsts.ConfigFileName));

        readonly XElement _loaded_config;

        public MyXmlReader()
        {
            _loadedConfig = LoadXml(XmlFilePath);
        }

        private XElement Load_xml(string xml_file_path)
        {
            try 
            {
                return XElement.Load(xmlFilePath);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error: {e.Message}");
            }  
            return null;
        }

        public string Read_xml(string xml_property)
        {
            return _loadedConfig.Descendants(xmlProperty).First().Value;
        }

        public string Read_xml(string xml_property, string xml_file_path)
        {
            var temp_config = LoadXml(xmlFilePath);
            return temp_config.Descendants(xmlProperty).First().Value;
        }
    }
}