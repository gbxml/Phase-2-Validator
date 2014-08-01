<%@ Page Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="TestDetailPage.aspx.cs" Inherits="XMLValidatorWeb.TestDetail" %>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
   <div class='container'>
        <div class="span12">
            <asp:Label ID="TestDetailLabelName" runat="server" Text=""></asp:Label>
        </div>
        <div class="well">
            <asp:Label ID="TestDetailLabelOverView" runat="server" Text=""></asp:Label>
        </div>
        <!--<div class="span2">
            <asp:Image  ID="TestDetailImage" runat="server" ImageUrl="~/Images/TmpImage.gif" AlternateText="test detail image" />
        </div>-->
        <div class = "well-sm">
        <asp:Label ID="TestDetailLabelResults" runat="server" Text=""></asp:Label>
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="ScriptPlaceHolder1" runat="server">
    <script type="text/javascript">
        $("#accordion").accordion({ active: false });

        $("#notaccordion").addClass("ui-accordion ui-accordion-icons ui-widget ui-helper-reset")
          .find("h3")
            .addClass("ui-accordion-header ui-helper-reset ui-state-default ui-corner-top ui-corner-bottom")
            .hover(function() { $(this).toggleClass("ui-state-hover"); })
            .click(function() {
              $(this)
                .toggleClass("ui-accordion-header-active ui-state-active ui-state-default ui-corner-bottom")
                .find("> .ui-icon").toggleClass("ui-icon-triangle-1-e ui-icon-triangle-1-s").end()
                .next().toggleClass("ui-accordion-content-active").slideToggle();
              return false;
            })
            .next()
              .addClass("ui-accordion-content  ui-helper-reset ui-widget-content ui-corner-bottom")
              .hide();

        $("#o4").hide();
        </script>
</asp:Content>
