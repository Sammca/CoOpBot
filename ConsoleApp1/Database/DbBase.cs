using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace CoOpBot.Database
{
    public abstract class DbBase
    {
        XmlDocument xmlDatabase = new XmlDocument();
        XmlNode root;
        int recId;

        public DbBase()
        {
            xmlDatabase.Load(FileLocations.xmlDatabase());
            root = xmlDatabase.DocumentElement;
        }

        public virtual void insert()
        {
            if (this.validateWrite() && this.validateInsert())
            {
                PropertyInfo[] properties = this.GetType().GetProperties();
                XmlNode recordXML;


                recordXML = CoOpGlobal.XML.createNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", this.newRecId(), "Record");


                foreach (PropertyInfo property in properties)
                {
                    XmlNode fieldNode = xmlDatabase.CreateElement(property.Name);

                    fieldNode.InnerText = $"{property.GetValue(this, null)}";

                    recordXML.AppendChild(fieldNode);
                }

                xmlDatabase.Save(FileLocations.xmlDatabase());
            }
            else
            {
                if (!this.validateWrite())
                {
                    Console.WriteLine($"{DBName()} write validation failed");
                }
                if (!this.validateInsert())
                {
                    Console.WriteLine($"{DBName()} insert validation failed");
                }
            }
            return;
        }

        public virtual void update()
        {
            if (this.validateWrite())
            {
                // TODO update
            }
            else
            {
                // Error
            }
            return;
        }
        
        public virtual void delete()
        {
            // TODO delete
            return;
        }

        public virtual bool validateWrite()
        {
            return true;
        }
        public virtual bool validateInsert()
        {
            return true;
        }

        public string DBName()
        {
            return this.GetType().Name;
        }

        public XmlNode DBRootNode()
        {
            return CoOpGlobal.XML.findOrCreateChild(xmlDatabase, root, this.DBName());
        }

        public string newRecId()
        {
            int recIdInt;

            do
            {
                recIdInt = CoOpGlobal.rng.Next(999999999);
            }
            while (this.findRecId(recIdInt) != null);

            return $"{recIdInt}";
        }

        public XmlNode findRecId(int recId)
        {
            XmlNode foundNode = null;

            foundNode = CoOpGlobal.XML.findNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", $"{recId}", "Record");

            return foundNode;
        }

        public bool exists(string fieldName, string value)
        {
            Boolean recordExists = false;
            Boolean stopSearching = false;
            XmlNode dbRoot = this.DBRootNode();
            IEnumerator recordEnumerator = dbRoot.GetEnumerator();


            while (recordEnumerator.MoveNext() && !stopSearching)
            {
                XmlNode curRecord = recordEnumerator.Current as XmlNode;

                IEnumerator fieldEnumerator = curRecord.GetEnumerator();
                while (fieldEnumerator.MoveNext() && !recordExists)
                {
                    XmlNode curField = fieldEnumerator.Current as XmlNode;

                    if (curField.Name == fieldName)
                    {
                        if (curField.InnerText == value)
                        {
                            recordExists = true;
                        }
                        stopSearching = true;
                        break;
                    }
                }
            }

            return recordExists;
        }
    }
}
