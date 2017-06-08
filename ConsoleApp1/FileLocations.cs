using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
