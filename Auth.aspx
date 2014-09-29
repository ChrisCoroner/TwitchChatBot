<% @Page Language="C#" AutoEventWireup="true"  CodeFile="Auth.aspx.cs" Inherits="Auth"%>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
	<title>QuizBot OAuth</title>
  		<script type="text/javascript">
  		    function JustDoIt() {
  		        var x = location.hash;
  		        PageMethods.Hash(x,onSucess, onError)
  		        function onSucess(result) {
  		            alert('OK');
  		        }
  		        function onError(result) {
  		            alert('Something wrong.');
  		        }
  		    }

		</script>
	</head>
	<body>
	<form id="Form" runat="server">
			QuizBot
		<asp:ScriptManager ID="ScriptManager" runat="server" EnablePageMethods="true" EnableScriptGlobalization="True"/>

	
		<script type="text/javascript">
			  	var x = location.hash;
  		        PageMethods.Hash(x,onSucess, onError);
  		        function onSucess(result) {
  		            alert(x);
  		        }
  		        function onError(result) {
  		            alert('Something wrong.');
  		        }
		</script>
      
	</form>
	</body> 
</html>