SchemaReportGenerator
=====================

SchemaReportGenerator Project is a very basic high level attempt to give you a snapshot of all Schema available in an Existing SDL Tridion system.
This project produces a consolidated report of all schema, their individual fields and type etc. The report generated is in HTML format the styling and markup of which is configurable through a configuration XML file. You can have a consolidated single page report on a web page or can have one HTML report per Schema stored on Disk in the form of an HTML file.

Further, this utility also allows you to generate corresponding Data Model for individual schema in the form of C# (.cs) file in an automated manner. Although the .cs file definitation is configurable through a configuration XML file and the Data Model .cs file will get generated automatically for all the fields of schema, there may be some Manual Effort in specific cases (specifically when there are Component/Multimedia Links and Embedded Schema fields in the schema) - The work is in progress for automating this as well.

How To Deploy:
1. 
