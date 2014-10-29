using System;
using System.Collections.Generic;
using CMS.SchemaManager;

namespace SchemaReport
{
    public partial class _Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SchemaParser parser = new SchemaParser();
            List<string> allSchemaInfo = parser.GenerateInfoForAllSchema();

            foreach (string schema in allSchemaInfo)
            {
                Response.Write(schema);
            }
        }
    }
}