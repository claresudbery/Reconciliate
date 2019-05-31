using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Interfaces.Constants
{
    public class XmlReader
    {
        public string XmlFilePath => ReadXml(ReconConsts.ConfigPathProperty, Path.Combine(ReconConsts.ConfigFilePath,ReconConsts.ConfigFileName));

        readonly XElement _loadedConfig;

        public XmlReader()
        {
            _loadedConfig = XElement.Load(XmlFilePath);
        }

        public string ReadXml(string xmlProperty)
        {
            return _loadedConfig.Descendants(xmlProperty).First().Value;
        }

        public string ReadXml(string xmlProperty, string xmlFilePath)
        {
            var temp_config = XElement.Load(xmlFilePath);
            return temp_config.Descendants(xmlProperty).First().Value;
        }
    }
}