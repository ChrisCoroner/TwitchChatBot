using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
    }

    protected void Page_LoadComplete(object sender, EventArgs e)
    {
        // Display the value of the HiddenField control.

        Console.WriteLine("The value of the HiddenField control is {0}", ValueHiddenField.Value); //ValueHiddenField.Value); //+ ValueHiddenField.Value + ".");

    }

    protected virtual void ValueHiddenField_ValueChanged(object sender, EventArgs e)
    {
        

    }


   // protected global::System.Web.UI.WebControls.HiddenField ValueHiddenField;
}