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
        public string recId;

        public DbBase()
        {
            xmlDatabase.Load(FileLocations.xmlDatabase());
            root = xmlDatabase.DocumentElement;
        }

        public DbBase createNewInstance()
        {
            var method = typeof(DbBase).GetMethod("newInstance", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(this.GetType());
            var value = method.Invoke(this, null);
            return (DbBase)value;
        }

        protected T newInstance<T>() where T : DbBase, new()
        {
            T newInstance = new T();
            return newInstance;
        }

        public virtual void insert()
        {
            if (recId == null)
            {
                recId = this.newRecId();
            }

            if (this.validateWrite() && this.validateInsert())
            {
                PropertyInfo[] properties = this.GetType().GetProperties();
                XmlNode recordXML;

                recId = this.newRecId();

                recordXML = CoOpGlobal.XML.createNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", recId, "Record");


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
            try
            {
                if (this.validateWrite())
                {
                    XmlNode updateNode = null;
                    PropertyInfo[] properties = this.GetType().GetProperties();

                    updateNode = CoOpGlobal.XML.findNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", $"{this.recId}", "Record");

                    foreach (PropertyInfo property in properties)
                    {
                        XmlNode fieldNode = CoOpGlobal.XML.updateOrCreateChildNode(xmlDatabase, updateNode, property.Name, $"{property.GetValue(this, null)}");
                    }
                }
                else
                {
                    Console.WriteLine($"{DBName()} write validation failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
            if (this.existsRecId(int.Parse(recId)))
            {
                return false;
            }
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

        public void setRecId(string recIdToSet)
        {
            recId = recIdToSet;
        }

        public string newRecId()
        {
            int recIdInt;

            do
            {
                recIdInt = CoOpGlobal.rng.Next(999999999);
            }
            while (this.existsRecId(recIdInt));

            return $"{recIdInt}";
        }

        public bool existsRecId(int recIdSearch)
        {
            XmlNode foundNode = null;

            foundNode = CoOpGlobal.XML.findNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", $"{recIdSearch}", "Record");

            return (foundNode != null);
        }

        public DbBase findRecId(string recIdSearch)
        {
            XmlNode foundRecord = null;
            DbBase retObject = null;

            foundRecord = CoOpGlobal.XML.findNodeWithAttribute(xmlDatabase, this.DBRootNode(), "RecId", $"{recIdSearch}", "Record");
            if (foundRecord != null)
            {
                // Initilaise record object
                retObject = this.createNewInstance();
                // Set Rec Id separately because it isn't a property, just a variable
                retObject.recId = recIdSearch;
                IEnumerator fieldEnumerator = foundRecord.GetEnumerator();

                // Assign field values to object properties
                while (fieldEnumerator.MoveNext())
                {
                    XmlNode curField = fieldEnumerator.Current as XmlNode;

                    retObject.GetType().GetProperty(curField.Name).SetValue(retObject, curField.InnerText);
                }
            }

            return retObject;
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

        public abstract string defaultFindField();

        public DbBase findFieldValue(string fieldName, string value)
        {
            DbBase retObject = null;
            Boolean recordExists = false;
            XmlNode dbRoot = this.DBRootNode();
            IEnumerator recordEnumerator = dbRoot.GetEnumerator();


            while (recordEnumerator.MoveNext() && !recordExists)
            {
                XmlNode curRecord = recordEnumerator.Current as XmlNode;

                IEnumerator fieldEnumerator = curRecord.GetEnumerator();
                IEnumerator fieldEnumeratorAssign = curRecord.GetEnumerator();
                while (fieldEnumerator.MoveNext() && !recordExists)
                {
                    XmlNode curField = fieldEnumerator.Current as XmlNode;

                    if (curField.Name == fieldName)
                    {
                        if (curField.InnerText == value)
                        {
                            recordExists = true;
                        }
                    }
                }

                if (recordExists)
                {
                    // Initilaise record object
                    retObject = this.createNewInstance();
                    // Set Rec Id separately because it isn't a property, just a variable
                    retObject.recId = curRecord.Attributes.GetNamedItem("RecId").Value;

                    // Assign field values to object properties
                    while (fieldEnumeratorAssign.MoveNext())
                    {
                        XmlNode curField = fieldEnumeratorAssign.Current as XmlNode;
                        
                        retObject.GetType().GetProperty(curField.Name).SetValue(retObject, curField.InnerText);
                    }
                }
            }

            return retObject;
        }

        public virtual DbBase find(string value)
        {
            return this.findFieldValue(this.defaultFindField(), value);
        }
    }
}
