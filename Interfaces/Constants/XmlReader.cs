using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Interfaces.Constants
{
    public class MyXmlReader
    {
        public string Xml_file_path => Read_xml(
            FilePathConsts.ConfigPathProperty, 
            Path.Combine(FilePathConsts.ConfigFilePath, FilePathConsts.ConfigFileName));

        private readonly XElement _loaded_config;

        public MyXmlReader()
        {
            _loaded_config = Load_xml(Xml_file_path);
        }

        private XElement Load_xml(string xml_file_path)
        {
            try 
            {
                return XElement.Load(xml_file_path);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Error: {e.Message}");
            }  
            return null;
        }

        public string Read_xml(string xml_property)
        {
            return _loaded_config.Descendants(xml_property).First().Value;
        }

        public string Read_xml(string xml_property, string xml_file_path)
        {
            var temp_config = Load_xml(xml_file_path);
            return temp_config.Descendants(xml_property).First().Value;
        }
    }
}