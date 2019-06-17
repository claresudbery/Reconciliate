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

        readonly XElement _loaded_config;

        public MyXmlReader()
        {
            _loaded_config = Load_xml(Xml_file_path);
        }

        private XElement Load_xml(string xmlFilePath)
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

        public string Read_xml(string xmlProperty)
        {
            return _loaded_config.Descendants(xmlProperty).First().Value;
        }

        public string Read_xml(string xmlProperty, string xmlFilePath)
        {
            var temp_config = Load_xml(xmlFilePath);
            return temp_config.Descendants(xmlProperty).First().Value;
        }
    }
}