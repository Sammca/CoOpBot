using System;
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
