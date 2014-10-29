using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml;
using Tridion.ContentManager;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.CoreService.Client;

namespace CMS.SchemaManager
{
    public enum WriteOperation
    {
        SchemaDefinition,
        DataModelDefinition
    }

    public class SchemaParser
    {
        private const string CONFIGURATION_PATH = "Configuration.xml";
        private const string DISPLAY_FORMAT_XPATH = "/schema-parser/html-format";
        private const string DATAMODEL_FORMAT_XPATH = "/schema-parser/datamodel-format";
        private const string AUTHOR_NAME_XPATH = "/schema-parser/author";
        private const string NAMESPACE_XPATH = "/schema-parser/namespace";
        private const string SCHEMADEF_FILEPATH_XPATH = "/schema-parser/schemadef-filepath";
        private const string DATAMODEL_FILEPATH_XPATH = "/schema-parser/datamodel-filepath";
        private const string IS_GENERATE_DATAMODEL_XPATH = "/schema-parser/generate-datamodel";
        private const string CORESERVICE_URL_XPATH = "/schema-parser/coreservice-url";
        private const string PUBLICATION_URI_XPATH = "/schema-parser/publication-uri";

        string format = string.Empty;
        string classDef = string.Empty;
        string author = string.Empty;
        string namespace_string = string.Empty;
        string schemaDef_FilePath = string.Empty;
        string dataModel_FilePath = string.Empty;
        string coreServiceURL = string.Empty;
        string bbFolderURI = string.Empty;
        string formattedSchemaInfo = string.Empty;
        string isGenerateDataModel = string.Empty;

        public SchemaParser()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(string.Concat(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_PATH));

            XmlNode node = doc.SelectSingleNode(DISPLAY_FORMAT_XPATH);

            format = node != null ? node.InnerXml : string.Empty;
            if (!string.IsNullOrEmpty(format))
            {
                format = StripCdata(format);
            }

            node = doc.SelectSingleNode(DATAMODEL_FORMAT_XPATH);
            classDef = node != null ? node.InnerXml : string.Empty;
            if (!string.IsNullOrEmpty(format))
            {
                classDef = StripCdata(classDef);
            }

            node = doc.SelectSingleNode(AUTHOR_NAME_XPATH);
            author = node != null ? node.InnerXml : string.Empty;

            node = doc.SelectSingleNode(NAMESPACE_XPATH);
            namespace_string = node != null ? node.InnerXml : string.Empty;

            node = doc.SelectSingleNode(SCHEMADEF_FILEPATH_XPATH);
            schemaDef_FilePath = node != null ? string.Concat(AppDomain.CurrentDomain.BaseDirectory, node.InnerXml) : string.Empty;


            node = doc.SelectSingleNode(DATAMODEL_FILEPATH_XPATH);
            dataModel_FilePath = node != null ? string.Concat(AppDomain.CurrentDomain.BaseDirectory, node.InnerXml) : string.Empty;

            node = doc.SelectSingleNode(CORESERVICE_URL_XPATH);
            coreServiceURL = node != null ? node.InnerXml : string.Empty;

            node = doc.SelectSingleNode(PUBLICATION_URI_XPATH);
            bbFolderURI = node != null ? node.InnerXml : string.Empty;

            node = doc.SelectSingleNode(IS_GENERATE_DATAMODEL_XPATH);
            isGenerateDataModel = node != null ? node.InnerXml : string.Empty;
        }

        public void ParseSchema(Schema subject)
        {
            try
            {
                string formattedInfo = string.Format(format, subject.Title + " Schema", subject.Description, subject.Description, subject.GetType().Name, subject.RootElementName, subject.Path);
                string xPath = "tcm:Schema/tcm:Data/tcm:XSD/";
                string formattedSchemaInfo = ParseSchemaXML(subject.ToXml().OwnerDocument.InnerXml, subject.RootElementName, subject.Title, xPath);
                WriteToFile(formattedSchemaInfo, WriteOperation.SchemaDefinition, subject.Title);
            }
            catch (Exception ex)
            {
            }
        }

        public void ParseSchema(SchemaData subject)
        {
            try
            {
                string formattedInfo = string.Format(format, subject.Title + " Schema", subject.Description, subject.Description, subject.Purpose.Value, subject.RootElementName, subject.Id);
                string xPath = string.Empty;
                formattedSchemaInfo = string.Empty;
                formattedSchemaInfo = ParseSchemaXML(subject.Xsd, subject.RootElementName, subject.Title, xPath);
                if (!string.IsNullOrEmpty(formattedSchemaInfo))
                {
                    formattedInfo = formattedInfo.Replace("<VISIBILITY>", "visibility:visible");
                }
                else
                {
                    formattedInfo = formattedInfo.Replace("<VISIBILITY>", "visibility:hidden");
                }
                formattedSchemaInfo = formattedInfo.Replace("<FIELD_DETAILS>", formattedSchemaInfo);
                WriteToFile(formattedSchemaInfo, WriteOperation.SchemaDefinition, subject.Title);
            }
            catch (Exception ex)
            {
            }
        }

        private string ParseSchemaXML(string xml, string xmlRootName, string dataModelFileName, string rootXPath)
        {
            StringBuilder fieldBuilder = new StringBuilder();
            if (!string.IsNullOrEmpty(xml))
            {
                XmlDocument domDoc = new XmlDocument();
                domDoc.LoadXml(xml);
                XmlNamespaceManager ns = new XmlNamespaceManager(domDoc.NameTable);
                ns.AddNamespace("tcm", "http://www.tridion.com/ContentManager/5.0");
                ns.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                ns.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");
                ns.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");

                Dictionary<string, string> fieldDictionary = new Dictionary<string, string>();

                XmlNodeList nodeList = domDoc.SelectNodes(string.Concat(rootXPath, "xsd:schema/xsd:annotation/xsd:appinfo/tcm:Labels/tcm:Label"), ns);

                StringBuilder dataMembers = new StringBuilder();

                foreach (XmlNode node in nodeList)
                {
                    if (node.Attributes["Metadata"].Value == "false")
                    {
                        fieldDictionary.Add(node.Attributes["ElementName"].Value, node.InnerText);


                        string temp = @"
                                    [XmlElement('{0}')]
                                    [DataMember]
                                    public string {1} {2}" + Environment.NewLine;

                        string rootName = node.Attributes["ElementName"].Value;
                        string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(node.Attributes["ElementName"].Value.ToLower());
                        string propertySuffix = @"{ get; set; }";

                        dataMembers.Append(string.Format(temp, rootName, propertyName, propertySuffix));
                    }

                    string schemaClassDef = classDef;
                    schemaClassDef = schemaClassDef.Replace("<SCHEMA_NAME_PASCAL_CASE>", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(xmlRootName.ToLower()));
                    schemaClassDef = schemaClassDef.Replace("<SCHEMA_XML_ROOT_NAME>", xmlRootName);
                    schemaClassDef = schemaClassDef.Replace("<DATA_MEMBER_LIST>", dataMembers.ToString());
                    schemaClassDef = schemaClassDef.Replace("<CREATE_DATE>", DateTime.Now.ToShortTimeString());
                    schemaClassDef = schemaClassDef.Replace("<AUOTHOR_NAME>", author);
                    schemaClassDef = schemaClassDef.Replace("<NAMESPACE_NAME>", namespace_string);
                    if (isGenerateDataModel == "1")
                    {
                        WriteToFile(schemaClassDef.Replace("'", "\""), WriteOperation.DataModelDefinition, dataModelFileName);
                    }
                }
                foreach (KeyValuePair<string, string> kvPair in fieldDictionary)
                {
                    string fieldItems = "<tr align='center'><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td align='left'>{5}</td></tr>";
                    string xmlName = kvPair.Key;
                    string fieldName = kvPair.Value;
                    string fieldType = "-";
                    string isMandatory = "-";
                    string maxLengthValidation = "-";
                    string additionalInfo = "-";

                    XmlNode complexField = domDoc.SelectSingleNode(string.Concat(rootXPath, "xsd:schema/xsd:element/xsd:complexType/xsd:sequence/xsd:element[@name='" + xmlName + "']"), ns);

                    if (complexField.Attributes["type"] == null)
                    {
                        XmlAttribute attr = domDoc.CreateAttribute("type");
                        attr.Value = "In-Line Schema List";
                        complexField.Attributes.Append(attr);
                    }

                    if (complexField != null)
                    {
                        fieldType = complexField.Attributes["type"].Value;

                        isMandatory = complexField.Attributes["minOccurs"].Value == "1" ? "Yes" : "No";

                        switch (complexField.Attributes["type"].Value)
                        {
                            case "xsd:normalizedString":
                                fieldType = "Text";
                                break;
                            case "tcmi:SimpleLink":
                                if (complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:linktype", ns) != null)
                                {
                                    fieldType = complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:linktype", ns).InnerText;
                                    string addInfo = string.Empty;
                                    if (complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:AllowMultimediaLinks", ns) != null)
                                    {
                                        string temp1 = complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:AllowMultimediaLinks", ns).InnerText == "false" ? "No" : "Yes";
                                        addInfo = "<b>Allow Multimedia Links:</b> " + temp1 + "<br />";
                                    }

                                    XmlNode allowedSchemaList = complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:AllowedTargetSchemas", ns);
                                    if (allowedSchemaList != null)
                                    {
                                        XmlNodeList schemaList = allowedSchemaList.SelectNodes("tcm:TargetSchema", ns);
                                        if (schemaList != null)
                                        {
                                            string temp2 = "<b>Allowed Schema:</b>" + "<br />";
                                            foreach (XmlNode schema in schemaList)
                                            {
                                                string schemaURI = schema.Attributes["xlink:title"].Value;
                                                temp2 = temp2 + schemaURI + "<br />";
                                            }
                                            addInfo += temp2;
                                        }
                                    }
                                    additionalInfo = addInfo;
                                }
                                break;

                            case "xsd:decimal":
                                fieldType = "Number";
                                break;

                            case "xsd:dateTime":
                                fieldType = "DateTime";
                                break;

                            case "tcmi:XHTML":
                                fieldType = "Text";
                                additionalInfo = "RTF Allowed <br /> <b>Size:</b> {0}";
                                if (complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:Size", ns) != null)
                                {
                                    additionalInfo = string.Format(additionalInfo, complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:Size", ns).InnerText);
                                }
                                break;
                            case "In-Line Schema List":
                                XmlNode inLineNode = complexField.SelectSingleNode("xsd:simpleType/xsd:restriction", ns);
                                if (inLineNode != null && inLineNode.Attributes != null && inLineNode.Attributes.Count > 0)
                                {
                                    if (inLineNode.Attributes["base"] != null)
                                    {
                                        switch (inLineNode.Attributes["base"].Value)
                                        {
                                            case "xsd:decimal":
                                                fieldType = "Number";
                                                break;
                                            case "xsd:normalizedString":
                                                fieldType = "Text";
                                                break;
                                            case "xsd:dateTime":
                                                fieldType = "DateTime";
                                                break;
                                        }
                                        additionalInfo = "Values will be selected from an In-Line Schema List <br /> <b>List Type:</b> " + complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:listtype", ns).InnerText;
                                    }
                                }
                                break;

                            default:
                                if (fieldType.Contains("category:"))
                                {
                                    string[] temp = fieldType.Split(':');
                                    fieldType = "Text";
                                    additionalInfo = "Values will be selected from List <br /> <b>List Type:</b> {0} <br /> <b>Category:</b>{1}";
                                    if (complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:listtype", ns) != null)
                                    {
                                        if (temp != null && temp.Length > 1)
                                        {
                                            additionalInfo = string.Format(additionalInfo, complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:listtype", ns).InnerText, temp[1].Trim());
                                        }
                                    }
                                }
                                else if (complexField.SelectSingleNode("xsd:annotation/xsd:appinfo/tcm:EmbeddedSchema", ns) != null)
                                {
                                    string allowMultiple = complexField.Attributes["maxOccurs"].Value == "1" ? "No" : "Yes";
                                    string schemaName = fieldType;
                                    fieldType = "Embedded Schema";
                                    additionalInfo = "<b>Embedded Schema:</b>{0} <br /> <b>Allow Multiple:</b> {1}";
                                    additionalInfo = string.Format(additionalInfo, schemaName, allowMultiple);
                                }
                                break;
                        }

                    }
                    fieldBuilder.Append(string.Format(fieldItems, xmlName, fieldName, fieldType, isMandatory, maxLengthValidation, additionalInfo));
                }
            }
            return fieldBuilder.ToString();
        }

        private void WriteToFile(string contentToWrite, WriteOperation operationType, string fileName)
        {
            string dirPath = string.Empty;
            string extension = string.Empty;
            switch (operationType)
            {
                case WriteOperation.SchemaDefinition:
                    dirPath = schemaDef_FilePath;
                    extension = ".html";
                    break;
                case WriteOperation.DataModelDefinition:
                    dirPath = dataModel_FilePath;
                    extension = ".cs";
                    break;
            }

            string filePath = string.Concat(dirPath, fileName, extension);

            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
            {
                file.WriteLine(contentToWrite);
                file.Close();
            }
        }

        private static string StripCdata(string input)
        {
            if (input == null)
                return String.Empty;

            String output = input;

            if (input.IndexOf("<![CDATA[") == 0)
            {
                output = input.Substring(9, input.Length - 9);
                if (output.IndexOf("]]>") == output.Length - 3)
                    output = output.Substring(0, output.Length - 3);
            }

            return output;
        }

        public SessionAwareCoreServiceClient GetCoreServiceClient()
        {
            var endpoint = new EndpointAddress(coreServiceURL);
            var binding = new WSHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647
                }
            };
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient(binding, endpoint);

            return client;
        }

        public List<string> GenerateInfoForAllSchema()
        {
            List<string> allSchema = new List<string>();

            TcmUri uri = new TcmUri(bbFolderURI);

            SessionAwareCoreServiceClient client = GetCoreServiceClient();

            RepositoryItemsFilterData filter = new RepositoryItemsFilterData();


            filter.ItemTypes = new[] { Tridion.ContentManager.CoreService.Client.ItemType.Schema };
            filter.Recursive = true;
            filter.BaseColumns = Tridion.ContentManager.CoreService.Client.ListBaseColumns.Id;
            
            IdentifiableObjectData[] schemas = client.GetList(bbFolderURI, filter);

            foreach (SchemaData schema in schemas)
            {
                SchemaData sch = (SchemaData)client.Read(schema.Id, null);
                ParseSchema(sch);
                allSchema.Add(formattedSchemaInfo);
            }

            return allSchema;
        }
    }
}
