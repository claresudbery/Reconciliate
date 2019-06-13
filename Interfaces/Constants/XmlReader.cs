using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Interfaces.Constants
{
    public class XmlReader
    {
        public string XmlFilePath => ReadXml(
            FilePathConsts.ConfigPathProperty, 
            Path.Combine(FilePathConsts.ConfigFilePath, FilePathConsts.ConfigFileName));

        readonly XElement _loadedConfig;

        public XmlReader()
        {
            _loadedConfig = LoadXml(XmlFilePath);
        }

        private XElement LoadXml(string xmlFilePath)
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

        public string ReadXml(string xmlProperty)
        {
            return _loadedConfig.Descendants(xmlProperty).First().Value;
        }

        public string ReadXml(string xmlProperty, string xmlFilePath)
        {
            var temp_config = LoadXml(xmlFilePath);
            return temp_config.Descendants(xmlProperty).First().Value;
        }
    }
}