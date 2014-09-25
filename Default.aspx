<% @Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="Default"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">



<html xmlns="http://www.w3.org/1999/xhtml">
	<head runat="server">


	<title>QuizBot OAuth</title>
	</head>
	<body>
    <form id="Form" runat="server">
	  QuizBot
        <script type="text/javascript">
            function GetHash() {
                return location.hash;
            }

        </script>
	  <asp:HiddenField id="ValueHiddenField" Value ="GetHash();"
              runat="server"/>
    
	  <p id="demo"></p>
      <p id="demo1"></p>
	  <script>
		  var x = location.hash;
		  document.getElementById("demo").innerHTML = x;
		  document.getElementById("<%=ValueHiddenField.ClientID%>").value= x;
		  document.getElementById("demo1").innerHTML = document.getElementById("<%=ValueHiddenField.ClientID%>").value;
      </script>
      
    </form>
	</body> 
</html>