<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true"
    CodeBehind="TestPage.aspx.cs" Inherits="XMLValidatorWeb.Pages.TestPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <script language="javascript" type="text/javascript">
        function openNewWindow(url) {
            var w = window.open(url);
            w.focus();
        } 
    </script>
    <script language="javascript" type="text/javascript">
        function Clickheretoprint() {
            var disp_setting = "toolbar=yes,location=no,directories=yes,menubar=yes,";
            disp_setting += "scrollbars=yes,width=650, height=600, left=100, top=25";
            var content_vlue = document.getElementById("print_content").innerHTML;

            var docprint = window.open("", "", disp_setting);
            docprint.document.open();
            docprint.document.write('<html><head><title>Inel Power System</title>');
            docprint.document.write('</head><body onLoad="self.print()"><center>');
            docprint.document.write(content_vlue);
            docprint.document.write('</center></body></html>');
            docprint.document.close();
            docprint.focus();
        }
    </script>

    <div class="container well-lg">
        <div class="container">
            <div class="container well">
                <asp:Panel ID="TestSummuryPanel" runat="server"></asp:Panel>
            </div>
            <div class="container well">
                <div class="well-sm">
                    <h3>Choose gbXML Schema:</h3>
                    <select id="VersionDropDownList" runat="server" class="form-control">
                        <option value="10">Green Building XML Version 5.10</option>
                        <option value="11">Green Building XML Version 5.11</option>
                        <option value="12" selected="selected">Green Building XML Version 5.12</option>
                    </select>
                </div> <!-- /Report Schema Selection Well -->
                <div class="well-sm">
                    <h3>Upload Your gbXML File Here:</h3> 
                    <div class="navbar">
                        <div class="navbar-brand pull-left">
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="Fileupload" class="btn" />
                        </div>
                        <div class="btn-group pull-right">
                            <asp:Button class="btn btn-success" ID="upLoadButton" runat="server" Text="View Report"
                                OnClick="upLoadButton_Click1" />
                        </div>
                    </div>
                </div> <!-- /File Upload Well -->
            </div>
            <div id="ResultsSections" runat="server" class="container well">
                <div id="print_content">
                    <div class="btn-group pull-right">
                        <asp:Button class="btn btn-default" ID="DownloadLogButton" runat="server" Text="Download Log File"
                            OnClick="DownloadLogButton_Click" Visible="False" />
                        <a class="btn btn-default" Id="PrintFriendly" runat="server" href="javascript:Clickheretoprint()" Visible="False">Print Friendly</a>
                    </div> <!-- /Report Button Group -->
                    <div class="well-sm">
                        <asp:Label ID="ResultSummaryLabel" runat="server" Text=""></asp:Label>
                    </div>
                    <div class="well-sm">
                        <asp:Panel ID="TestResultPanel" runat="server"></asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <asp:Label ID="LogLabel" runat="server" Visible="False"></asp:Label>
    <asp:Label ID="TableLabel" runat="server" Visible="False"></asp:Label>
</asp:Content>
