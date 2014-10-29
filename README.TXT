SchemaReportGenerator
=====================

SchemaReportGenerator Project is a very basic high level attempt to give you a snapshot of all Schema available in an Existing SDL Tridion system.
This project produces a consolidated report of all schema, their individual fields and type etc. The report generated is in HTML format the styling and markup of which is configurable through a configuration XML file. You can have a consolidated single page report on a web page or can have one HTML report per Schema stored on Disk in the form of an HTML file.

Further, this utility also allows you to generate corresponding Data Model for individual schema in the form of C# (.cs) file in an automated manner. Although the .cs file definitation is configurable through a configuration XML file and the Data Model .cs file will get generated automatically for all the fields of schema, there may be some Manual Effort in specific cases (specifically when there are Component/Multimedia Links and Embedded Schema fields in the schema) - The work is in progress for automating this as well.

How To Deploy:
1. Download and Unzip the File: SchemaReportGenerator\Deploy\SchemaReportGenerator.zip
2. In the IIS (Local Machine or CMS Server or anywhere): Create a Website/WebApplication and target it to the unzipped SchemaReportGenerator folder
3. Change the Application Pool Settings, Website Binding etc. if needed so that the Website/Web Application can be browsed
4. In the SchemaReportGenerator Folder, locate the Configuration.xml file and open it for edit - Locate the <coreservice-url> node and update it to reflect the Core Service URL for your SDL Tridion environment. Also locate the <publication-uri> node and update it to provide the full Publication URI of the publication for which you want to generate the report.
5. Browse the Webpage - SchemaDetails.aspx. This will show all the schema in the publication (specified by the Publication URI of <publication-uri> node in Configuration.xml) on the webpage - This page may take some time to load depending on the number of schema in the system as there is no AJAX implementation. Additionally this will generate individual html files and .cs files at the path specified in the <schemadef-filepath> and <datamodel-filepath> nodes in the Configuration.xml file

Configuration.XML file Elaborated:

<html-format>: Specify the format of the HTML generated. Please note following strings should be changes in this node - <VISIBILITY>, <FIELD_DETAILS>

<datamodel-format>: Specify the format of the .cs file generated. Please note following strings should be changes in this node - <SCHEMA_NAME_PASCAL_CASE>, <AUTHOR_NAME>, <CREATE_DATE>, <NAMESPACE_NAME>, <SCHEMA_XML_ROOT_NAME>, <DATA_MEMBER_LIST>

<author>: Specify the author of the .cs file. The <AUTHOR_NAME> field in the <datamodel-format> node will be replaced by the value of this node

<namespace>: Specify the namespace for the .cs file. The <NAMESPACE_NAME> field in the <datamodel-format> node will be replaced by the value of this node

<schemadef-filepath>: Specify the path where individual .html file for schema will be generated

<datamodel-filepath>: Specify the path where individual .cs file for schema data model will be generated

<generate-datamodel>: Specify whether to generate the data models for each schema or not. The data models for schema will be generated only if the value of this node is set to 1

<coreservice-url>: Core Service URL of the SDL Tridion Environment

<publication-uri>: Publication URI of the publication from which the details of schema to extract
