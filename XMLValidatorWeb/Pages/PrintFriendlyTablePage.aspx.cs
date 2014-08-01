using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace XMLValidatorWeb
{
    public partial class PrintFriendlyTablePage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (PrintFriendlyTablePageLabel != null)
            {
                if (Session["table"] == null)
                    Response.Redirect(@"~/");

                PrintFriendlyTablePageLabel.Text = Session["table"].ToString();
            }

        }
    }
}