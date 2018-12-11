using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace BAFactory.Moira.Core
{
    public abstract class XmlConfigurationParser
    {
        protected readonly string customNamespace;
        protected readonly string customNamespacePrefix;

        protected XmlElement documentElement;
        protected XmlNamespaceManager nmspcManager;

        public XmlConfigurationParser(string customNamespace, string customPrefix)
        {
            this.customNamespace = customNamespace;
            this.customNamespacePrefix = customPrefix;
        }

        protected bool LoadConfigFile(string configFileName)
        {
            bool result = false;
            XmlDocument configDocument = new XmlDocument();

            try
            {
                using (XmlReader reader = XmlReader.Create(configFileName))
                {
                    configDocument.Load(reader);
                }
                documentElement = configDocument.DocumentElement;

                nmspcManager = new XmlNamespaceManager(configDocument.NameTable);
                nmspcManager.AddNamespace(customNamespacePrefix, customNamespace);

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }

        protected XmlNode GetDocumentNode(string p)
        {
            string qualifiedPath = string.Concat(customNamespacePrefix, ":", p).Replace("/", string.Concat("/", customNamespacePrefix, ":"));
            return documentElement.SelectSingleNode(qualifiedPath, nmspcManager);
        }

        protected void MoveNavigatorToChildNode(ref XPathNavigator nav, string nodeName)
        {
            nav.MoveToChild(nodeName, customNamespace);
        }

        protected void MoveNavigatorToParentNode(ref XPathNavigator nav)
        {
            MoveNavigatorToParentNode(ref nav, 1);
        }
        protected void MoveNavigatorToParentNode(ref XPathNavigator nav, uint levels)
        {
            for (int i = 0; i < levels; ++i)
            {
                nav.MoveToParent();
            }
        }

        protected void MoveNavigatorToSiblingNode(ref XPathNavigator nav, string nodeName)
        {
            nav.MoveToParent();
            MoveNavigatorToChildNode(ref nav, nodeName);
        }
    }
}
