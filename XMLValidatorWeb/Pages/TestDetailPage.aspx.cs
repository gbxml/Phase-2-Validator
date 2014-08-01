using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DOEgbXML;

namespace XMLValidatorWeb
{
    public partial class TestDetail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (TestDetailLabelOverView != null)
            {
                if (Request.QueryString["type"] != "Error")
                {
                    // List of reports
                    List<DOEgbXMLPhase2Report> reportlist = new List<DOEgbXMLPhase2Report>();
                    if (Session["reportList"] == null || Request.QueryString["type"] == null)
                    {
                        Response.Redirect(@"~/");
                    }
                    reportlist = (List<DOEgbXMLPhase2Report>)Session["reportList"];

                    #region Get Current Report
                    //looking for the right report from the list
                    int testType = 0;
                    int subType = -1;

                    if (Request.QueryString["type"] != null)
                    {
                        try
                        {
                            testType = (int)Convert.ToInt32(Request.QueryString["type"]);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    if (Request.QueryString["subtype"] != null)
                    {
                        try
                        {
                            subType = (int)Convert.ToInt32(Request.QueryString["subtype"]);
                        }
                        catch
                        {
                            return;
                        }
                    }

                    DOEgbXMLPhase2Report rightReport = new DOEgbXMLPhase2Report();
                    foreach (DOEgbXMLPhase2Report report in reportlist)
                    {
                        if (report.testType == (TestType)testType)
                        {
                            if (report.subTestIndex == -1 || report.subTestIndex == subType)
                            {
                                rightReport = report;
                            }
                        }
                    }
                    #endregion Get Current Report

                    #region Title
                    string title = rightReport.testType.ToString();
                    title = title.Replace("_", " ");
                    if (subType != -1)
                    {
                        title += " " + subType;
                    }
                    TestDetailLabelName.Text += "<h2>" + title + "</h2>";
                    #endregion Title
                    #region Description
                    TestDetailLabelOverView.Text += "<h4>Test Summary:</h4><p>" + rightReport.testSummary + "</p>";
                    #endregion Description
                    #region Message Summary
                    var passTest = rightReport.TestPassedDict.Values;
                    bool individualTestBool = true;
                    foreach (bool testResult in passTest)
                    {
                        if (testResult == false)
                        {
                            individualTestBool = false;
                            break;
                        }
                    }

                    string output = "<div class='panel panel-default'><div class='panel-heading'><h4 class='panel-title'>Test Results:</h4></div><div class='panel-body'>";
                    if (rightReport.passOrFail && individualTestBool)
                    {
                        output += "<p class='text-success panel-body'>" + rightReport.longMsg + "</p>";
                    }
                    else
                    {
                        output += "<p class='text-danger panel-body'>" + rightReport.longMsg + "</p>";
                    }
                    #endregion Message Summary
                    #region Message Detail
                    //message list, print out each message in the list if there are any
                    if (rightReport.MessageList.Count() > 0)
                    {
                        output += "<div id='notaccordion'>";
                        foreach (KeyValuePair<string, List<string>> message in rightReport.MessageList)
                        {
                            bool panelError = false;
                            string currentKeyOutput = "";
                            #region Panel Body
                            string currentBodyOutput = "<div>";
                            foreach (string finding in message.Value)
                            {
                                if (finding.Substring(0, 5) == "PASS:" || finding.Substring(0, 14) == "PERFECT MATCH:")
                                {
                                    currentBodyOutput += "<p class='text-success'>" + finding + "</p>";
                                }
                                else
                                {
                                    currentBodyOutput += "<p class='text-danger'>" + finding + "</p>";
                                    panelError = true;
                                }
                            } 
                            currentBodyOutput += "</div>";
                            #endregion Panel Body
                            #region Panel Header
                            if (panelError)
                            {
                                currentKeyOutput += "<h3 style='color:Red'>" + message.Key + "</h3>";
                            }
                            else
                            {
                                currentKeyOutput += "<h3>" + message.Key + "</h3>";
                            }
                            #endregion Panel Header
                            output += currentKeyOutput + currentBodyOutput;
                        }
                        output += "</div>";
                    }
                    else if (rightReport.TestPassedDict.Count() > 0)
                    {
                        output += "<div id='notaccordion'>";
                        foreach (KeyValuePair<string, bool> pair in rightReport.TestPassedDict)
                        {
                            //output += "<p class='text-info'>" + pair.Key + ":" + pair.Value.ToString() + "</p>";
                            string currentKey = "";
                            if (currentKey == pair.Key)
                            {
                                output += "<p>" + pair.Value.ToString() + "</p>";
                            }
                            else
                            {
                                output += "<h3>" + pair.Key + "</h3>";
                                output += "<div><p>" + pair.Value.ToString() + "</p></div>";
                                currentKey = pair.Key;
                            }
                        }
                        output += "</div>";
                    }
                    output += "</div></div>";
                    #endregion Message Detail
                    TestDetailLabelResults.Text = output;
                }
                else
                {
                    TestDetailImage.Visible = false;
                    TestDetailLabelResults.Text = Session["table"].ToString();
                }
            }
        }
    }
}