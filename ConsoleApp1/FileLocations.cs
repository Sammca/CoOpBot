using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoOpBot
{
    public static class FileLocations
    {
        public static string xmlParameters()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CoOpBotParameters.xml";
        }

        public static string backupXML()
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
}
