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


    protected void Page_LoadComplete(object sender, EventArgs e)
    {
        // Display the value of the HiddenField control.
        //HiddenField hd = Request.Form["ValueHiddenField"];
        Console.WriteLine("The value of the HiddenField control is {0}", this.Page.Form.HasControls()); //ValueHiddenField.Value); //+ ValueHiddenField.Value + ".");
        foreach (Control i in this.Page.Form.Controls) {
            Console.WriteLine(i.ID);
        }
        HiddenField ctrl = (HiddenField)this.Page.Form.FindControl("ValueHiddenField");
        Console.WriteLine(ctrl.Value);
    }

    protected virtual void ValueHiddenField_ValueChanged(object sender, EventArgs e)
    {
        

    }


   // protected global::System.Web.UI.WebControls.HiddenField ValueHiddenField;
}