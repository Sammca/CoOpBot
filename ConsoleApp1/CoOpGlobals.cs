using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CoOpBot
{
    public static class FileLocations
    {
        public static string xmlParameters()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml";
        }

        public static string backupXMLParameters()
        {
            string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml";
            string backupPath = "";
            string[] pathArray;

            pathArray = fullPath.Split('\\');
            pathArray = pathArray.Take(pathArray.Length - 6).ToArray();

            for (int i = 0; i < pathArray.Length; i++)
            {
                if (i > 0)
                {
                    backupPath += '\\';
                }
                backupPath += pathArray[i];

            }

            backupPath += @"\CoOpBotParametersBAK.xml";

            return backupPath;
        }

        public static string xmlDatabase()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotDB.xml";
        }

        public static string backupXMLDatabase()
        {
            string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotDB.xml";
            string backupPath = "";
            string[] pathArray;

            pathArray = fullPath.Split('\\');
            pathArray = pathArray.Take(pathArray.Length - 6).ToArray();

            for (int i = 0; i < pathArray.Length; i++)
            {
                if (i > 0)
                {
                    backupPath += '\\';
                }
                backupPath += pathArray[i];

            }

            backupPath += @"\CoOpBotDBBAK.xml";

            return backupPath;
        }

        public static string gwItemNames()
        {
            string filename = @"\gwItemNames.xml";

            if (!File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + filename))
            {
                if (!File.Exists(FileLocations.gwItemNamesBackup()))
                {
                    XmlDocument gwItems = new XmlDocument();
                    gwItems.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                    XmlElement root = gwItems.CreateElement("gwItemNames");

                    gwItems.AppendChild(root);
                    gwItems.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + filename);
                }
                else
                {
                    XmlDocument gwItems = new XmlDocument();
                    gwItems.Load(FileLocations.gwItemNamesBackup());
                    gwItems.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + filename);

                    Console.WriteLine("GW item names file restored from backup");
                }
            }
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + filename;
        }

        public static string gwItemNamesBackup()
        {
            string fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\gwItemNames.xml";
            string backupPath = "";
            string[] pathArray;

            pathArray = fullPath.Split('\\');
            pathArray = pathArray.Take(pathArray.Length - 6).ToArray();

            for (int i = 0; i < pathArray.Length; i++)
            {
                if (i > 0)
                {
                    backupPath += '\\';
                }
                backupPath += pathArray[i];

            }

            backupPath += @"\gwItemNamesBAK.xml";

            return backupPath;
        }
    }

    public static class CoOpGlobal
    {
        // Global variables
        public static DateTime bootupDateTime { get; set; }

        public static Random rng { get; set; }

        public static class XML
        {
            public static XmlNode findOrCreateChild(XmlDocument file, XmlNode parent, string childName, string defaultValue = "")
            {
                XmlNode childNode;
                string filePath = new Uri(file.BaseURI).LocalPath;

                childNode = parent.SelectSingleNode($"descendant::{childName}");

                if (childNode == null)
                {
                    childNode = file.CreateElement(childName);

                    if (defaultValue != "")
                    {
                        childNode.InnerText = defaultValue;
                    }

                    parent.AppendChild(childNode);
                    file.Save(filePath);
                }

                return childNode;
            }

            public static XmlNode updateOrCreateChildNode(XmlDocument file, XmlNode parent, string childName, string newValue)
            {
                XmlNode childNode;
                string filePath = new Uri(file.BaseURI).LocalPath;

                childNode = parent.SelectSingleNode($"descendant::{childName}");

                if (childNode == null)
                {
                    childNode = file.CreateElement(childName);
                    childNode.InnerText = newValue;
                    parent.AppendChild(childNode);
                }
                else
                {
                    childNode.InnerText = newValue;
                }

                file.Save(filePath);

                return childNode;
            }

            public static XmlElement findNodeWithAttribute(XmlDocument file, XmlNode parentNode, string attributeName, string attributeValue, string childNodeName)
            {
                IEnumerator nodeEnumerator = parentNode.GetEnumerator();
                Boolean nodeExists = false;
                XmlElement foundNode = null;

                while (nodeEnumerator.MoveNext() && !nodeExists)
                {
                    XmlElement curNode = nodeEnumerator.Current as XmlElement;

                    if (curNode.GetAttribute(attributeName) == attributeValue)
                    {
                        foundNode = curNode;
                        nodeExists = true;
                    }
                }

                return foundNode;
            }

            public static XmlElement createNodeWithAttribute(XmlDocument file, XmlNode parentNode, string attributeName, string attributeValue, string childNodeName)
            {
                XmlElement foundNode = null;
                XmlElement newNode;
                string filePath = new Uri(file.BaseURI).LocalPath;

                newNode = file.CreateElement(childNodeName);
                newNode.SetAttribute(attributeName, attributeValue);

                foundNode = parentNode.AppendChild(newNode) as XmlElement;

                file.Save(filePath);

                return foundNode;
            }

            public static XmlElement findOrCreateNodeFromAttribute(XmlDocument file, XmlNode parentNode, string attributeName, string attributeValue, string childNodeName)
            {
                XmlElement foundNode = null;
                
                foundNode = findNodeWithAttribute(file, parentNode, attributeName, attributeValue, childNodeName);

                if (foundNode == null)
                {
                    foundNode = createNodeWithAttribute(file, parentNode, attributeName, attributeValue, childNodeName);
                }

                return foundNode;
            }
            
            /*public static XmlNode findChildNode(XmlDocument file, XmlNode parent, string childNodeName)
            {
                XmlNode newNode;
                string filePath = new Uri(file.BaseURI).LocalPath;

                parent.

                newNode = file.CreateElement(newNodeName) as XmlNode;
                newNode.InnerText = value;

                parent.AppendChild(newNode);
                file.Save(filePath);

                return newNode;
            }*/

            public static Boolean searchChildNodes(XmlDocument file, XmlNode parent, string searchValue)
            {
                XmlNodeList searchNodes = parent.ChildNodes;

                foreach (XmlNode childNode in searchNodes)
                {
                    if (childNode.InnerText == searchValue)
                    {
                        return true;
                    }
                }

                return false;
            }

            public static XmlNode createChildNode(XmlDocument file, XmlNode parent, string newNodeName, string value)
            {
                XmlNode newNode;
                string filePath = new Uri(file.BaseURI).LocalPath;

                newNode = file.CreateElement(newNodeName) as XmlNode;
                newNode.InnerText = value;

                parent.AppendChild(newNode);
                file.Save(filePath);

                return newNode;
            }
        }
    }

    static class LevenshteinDistance
    {
        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }
    }
}
