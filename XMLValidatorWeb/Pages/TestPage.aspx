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

    <div class="container well-lg">
        <div class="container">
            <div class="container well">
                <asp:Label ID="TestSummuryLabel" runat="server" Text=""></asp:Label>
            </div>
            <div class="container well">
                <div class="well-sm">
                    <h3>Choose Report Schema:</h3>
                    <select id="VersionDropDownList" runat="server" class="form-control">
                        <option value="10" selected="selected">Green Building XML Version 5.10</option>
                        <option value="11">Green Building XML Version 5.11</option>
                        <option value="12">Green Building XML Version 5.12</option>
                    </select>
                </div>
                <div class="well-sm">
                    <h3>Upload Your XML File Here:</h3> 
                    <div class="navbar">
                        <div class="navbar-brand pull-left">
                            <asp:FileUpload ID="FileUpload1" runat="server" CssClass="Fileupload" class="btn" />
                        </div>
                        <div class="btn-group pull-right">
                            <asp:Button class="btn btn-success" ID="upLoadButton" runat="server" Text="View Report"
                                OnClick="upLoadButton_Click1" />
                            <asp:Button class="btn" ID="downloadXMLButton" runat="server" Text="Download XML Report"
                                OnClick="downloadXMLButton_Click" Visible="false" />
                            <asp:Button class="btn" ID="downloadJSONButton" runat="server" Text="Download JSON Report"
                                OnClick="downloadJSONButton_Click" Visible="false" />
                            <asp:Button class="btn" ID="importXMLButton" runat="server" Text="Build Report from XML"
                                OnClick="importXMLButton_Click" Visible="false" />
                            <asp:Button class="btn" ID="importJSONButton" runat="server" Text="Build Report from JSON"
                                OnClick="importJSONButton_Click" Visible="false" />
                        </div>
                    </div>
                </div>
            </div>
            <div id="ResultsSections" runat="server" class="container well">
                <div class="btn-group pull-right">
                    <asp:Button class="btn" ID="DownloadLogButton" runat="server" Text="Download Log File"
                        OnClick="DownloadLogButton_Click" Visible="False" />
                    <asp:Button class="btn" ID="PrintFriendlyButton" runat="server" Text="Print Friendly"
                        OnClick="PrintFriendlyButton_Click" Visible="False" />
                </div>
                <div class="well-sm">
                    <asp:Label ID="ResultSummaryLabel" runat="server" Text=""></asp:Label>
                </div>
                <div class="well-sm">
                    <asp:Label ID="TestResultLabel" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>
    </div>
    <asp:Label ID="LogLabel" runat="server" Visible="False"></asp:Label>
    <asp:Label ID="TableLabel" runat="server" Visible="False"></asp:Label>
</asp:Content>
