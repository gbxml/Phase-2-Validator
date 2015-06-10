using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;
using System.Xml.Schema;
using Newtonsoft.Json;
using DOEgbXML;
using VectorMath;
using XMLValidatorWeb;

namespace XMLValidatorWeb.Pages
{
    public partial class TestPage : System.Web.UI.Page
    {
        //define tolerances for the tests
        static double coordtol = DOEgbXMLBasics.Tolerances.coordToleranceIP;
        public List<DOEgbXMLPhase2Report> ReportList = new List<DOEgbXMLPhase2Report>();
        public string log;
        public string table;  // TODO: Replace "table" string with outputs to a panel???
        string TestToRun = "GenericPhase2";
        DOEgbXMLTestCriteriaObject TestCriteria;
        DOEgbXMLTestDetail TestDetail;
        public string summaryTable;
        bool spaceBoundsPresent = false;
        bool overallPassTest = true;

        protected void Page_Load(object sender, EventArgs e)
        {
            ResultsSections.Visible = false;
            TestSummuryPanel.Controls.Add(new LiteralControl("<h4>This validation website allows you to upload your own gbXML files, and it performs the following 3 levels of verification:</h4>"));
            TestSummuryPanel.Controls.Add(new LiteralControl("<ol>"));
            TestSummuryPanel.Controls.Add(new LiteralControl("<li>Level 1: Checks for a properly formed XML file</li>"));
            TestSummuryPanel.Controls.Add(new LiteralControl("<li>Level 2: Checks for a properly formed gbXML file comparing it to the 5.10 and later schema versions</li>"));
            TestSummuryPanel.Controls.Add(new LiteralControl("<li>Level 3: Checks for the 16 standard test cases</li>"));
            TestSummuryPanel.Controls.Add(new LiteralControl("</ol>"));
        }
        protected void upLoadButton_Click1(object sender, EventArgs e)
        {
            string selectedVersionValue = "10";
            if (VersionDropDownList != null)
            {
                selectedVersionValue = VersionDropDownList.Value;
            }
            if (FileUpload1.HasFile)
            {
                if (FileUpload1.PostedFile.ContentType == "text/xml")
                {
                    #region Is XML File
                    //if there is a file
                    //valadate it by pass in input stream as xmlreader
                    Stream responseStream = FileUpload1.PostedFile.InputStream;
                    XmlReader xmlreader = XmlReader.Create(responseStream);

                    //validating xml with Phase 1 still.
                    Phase1_DOEgbXML.DOEgbXMLValidator val = new Phase1_DOEgbXML.DOEgbXMLValidator();
                    //Run the DOEgbXMLValidator...
                    if (!val.IsValidXmlEx(xmlreader, selectedVersionValue) || val.nErrors > 0 || val.nWarnings > 0)
                    {
                        //if it is not valid
                        #region Not Valid
                        if (PrintFriendlyButton != null)
                            PrintFriendlyButton.Visible = false;

                        if (DownloadLogButton != null)
                            DownloadLogButton.Visible = false;

                        //setup errorlog
                        string errorLog = "";
                        string errorDes = "";
                        if (val.nErrors > 0 || val.nWarnings > 0)
                        {
                            errorLog += "<p class='text-error'><div class='alert alert-danger'>" + "Find " + val.nErrors + " Errors and " + val.nWarnings + " Warnings <br/> <br/>" + val.Errors + "</div></p>";
                            errorDes = "Find ";
                            if (val.nErrors > 0)
                            {
                                errorDes += val.nErrors;
                                if (val.nWarnings > 0)
                                    errorDes += " Errors and";
                                else
                                    errorDes += " Errors";

                            }
                            if (val.nWarnings > 0)
                                errorDes += val.nWarnings + " Warnings";
                        }
                        else
                        {
                            errorLog += "<p class='text-error'><div class='alert alert-danger'>" + "Your XML File is severely deficient structurally.  It may be missing element tags or is not valid XML.  The test has failed. <br /><br/>" + val.BigError + "<br />" + "</div></p>";
                            errorDes = "Your XML File is severely deficient structurally.";
                        }
                        // Session.Add("table", errorLog);
                        Session["table"] = errorLog;

                        TestResultLabel.Text = "";
                        ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
                        ResultSummaryLabel.Text += "<table class='table'>";
                        ResultSummaryLabel.Text += "<tr class='alert alert-danger'>" +
                                        "<td>" + "gbXML schema Test" + "</td>" +
                                        "<td>" + errorDes + "</td>" +
                                        "<td>" + "Fail" + "</td>" +
                                        "<td>" + "<a href='TestDetailPage.aspx?type=Error' target='_blank'>" + "More Detail" + "</a>" + "</td>" +
                                        "</tr>";
                        ResultSummaryLabel.Text += "</table><br/>";
                        ResultsSections.Visible = true;
                        #endregion Not Valid
                    }
                    else if (val.nErrors == 0 && val.nWarnings == 0)
                    {
                        //if it is valid
                        #region Valid
                        // Show Results
                        ResultsSections.Visible = true;

                        //run test
                        XMLParser parser = new XMLParser();

                        responseStream.Position = 0;
                        XmlReader xmlreader2 = XmlReader.Create(responseStream);

                        //run through reports
                        ProcessValidXML(parser, xmlreader2);

                        //show summary table
                        ResultSummaryLabel.Text = summaryTable;
  
                        //show test section table // TODO: Replace with a panel?
                        TestResultLabel.Text = table;
                        
                        //store reportlist in session for TestDetailPage.
                        Session["reportList"] = ReportList;
 
                        LogLabel.Text = log;
                        TableLabel.Text = table;
                        //remove extra tag
                        TableLabel.Text = TableLabel.Text.Replace("<a href='PrintFriendlyTablePage.aspx' target='_blank'>", "");
                        TableLabel.Text = TableLabel.Text.Replace("</a>", "");
                        DownloadLogButton.Visible = true;
                        PrintFriendlyButton.Visible = true;
                        #endregion Valid
                    }
                    //this should never happen
                    else
                    {
                        ResultSummaryLabel.Text = "?????????something is very wrong";
                        TestResultLabel.Text = "";
                    }
                    #endregion Is XML File
                }
                //if the file type is not xml
                else
                {
                    #region Not XML File
                    if (PrintFriendlyButton != null)
                        PrintFriendlyButton.Visible = false;

                    if (DownloadLogButton != null)
                        DownloadLogButton.Visible = false;

                    ResultSummaryLabel.Text = "";
                    TestResultLabel.Text = "";
                    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
                    ResultSummaryLabel.Text += "<table class='table'>";
                    ResultSummaryLabel.Text += "<tr class='alert alert-danger'>" +
                                    "<td>" + "gbXML schema Test" + "</td>" +
                                    "<td>" + "You have not specified a right type of file." + "</td>" +
                                    "<td>" + "Fail" + "</td>" +
                                    "</tr>";
                    ResultSummaryLabel.Text += "</table><br/>";
                    ResultsSections.Visible = true;
                    #endregion Not XML File
                }
            }
            //if there is no file
            else
            {
                if (PrintFriendlyButton != null)
                    PrintFriendlyButton.Visible = false;

                if (DownloadLogButton != null)
                    DownloadLogButton.Visible = false;

                ResultSummaryLabel.Text = "";
                TestResultLabel.Text = "";
                ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
                ResultSummaryLabel.Text += "<table class='table'>";
                ResultSummaryLabel.Text += "<tr class='alert alert-danger'>" +
                                "<td>" + "gbXML schema Test" + "</td>" +
                                "<td>" + "You have not specified a file." + "</td>" +
                                "<td>" + "Fail" + "</td>" +
                                "</tr>";
                ResultSummaryLabel.Text += "</table><br/>";
                ResultsSections.Visible = true;
            }
        }

        #region Test Buttons
        protected void downloadXMLButton_Click(object sender, EventArgs e)
        {
            //if (FileUpload1.HasFile)
            //{
            //    if (FileUpload1.PostedFile.ContentType == "text/xml")
            //    {
            //        //if there is a file
            //        #region XML File
            //        //valadate it by pass in input stream as xmlreader
            //        Stream responseStream = FileUpload1.PostedFile.InputStream;
            //        XmlReader xmlreader = XmlReader.Create(responseStream);

            //        //validating xml
            //        DOEgbXMLValidator val = new DOEgbXMLValidator();
            //        //if it is not valid
            //        if (!val.IsValidXmlEx(xmlreader) || val.nErrors > 0 || val.nWarnings > 0)
            //        {
            //            if (PrintFriendlyButton != null)
            //                PrintFriendlyButton.Visible = false;

            //            if (DownloadLogButton != null)
            //                DownloadLogButton.Visible = false;

            //            //setup errorlog
            //            string errorLog = "";
            //            string errorDes = "";
            //            if (val.nErrors > 0 || val.nWarnings > 0)
            //            {
            //                errorLog += "<p class='text-error'><div class='alert alert-error'>" + "Find " + val.nErrors + " Errors and " + val.nWarnings + " Warnings <br/> <br/>" + val.Errors + "</div></p>";
            //                errorDes = "Find ";
            //                if (val.nErrors > 0)
            //                {
            //                    errorDes += val.nErrors;
            //                    if (val.nWarnings > 0)
            //                        errorDes += " Errors and";
            //                    else
            //                        errorDes += " Errors";

            //                }
            //                if (val.nWarnings > 0)
            //                    errorDes += val.nWarnings + " Warnings";
            //            }
            //            else
            //            {
            //                errorLog += "<p class='text-error'><div class='alert alert-error'>" + "Your XML File is severely deficient structurally.  It may be missing element tags or is not valid XML.  The test has failed. <br /><br/>" + val.BigError + "<br />" + "</div></p>";
            //                errorDes = "Your XML File is severely deficient structurally.";
            //            }
            //            // Session.Add("table", errorLog);
            //            Session["table"] = errorLog;

            //            TestResultLabel.Text = "";
            //        }
            //        //if it is valid
            //        else if (val.nErrors == 0 && val.nWarnings == 0)
            //        {
            //            //run test
            //            XMLParser parser = new XMLParser();

            //            responseStream.Position = 0;
            //            XmlReader xmlreader2 = XmlReader.Create(responseStream);

            //            /* Start Test */
            //            int xmlReport = 1;
            //            parser.StartReportDownloadTest(xmlreader2, "Test1", Page.User.Identity.Name, xmlReport);
            //        }
            //        //this should never happens
            //        else
            //        {
            //            ResultSummaryLabel.Text = "?????????something is very wrong";
            //            TestResultLabel.Text = "";
            //        }
            //        #endregion XML File
            //    }
            //    //if the file type is not xml
            //    else
            //    {
            //        #region Not XML File
            //        if (PrintFriendlyButton != null)
            //            PrintFriendlyButton.Visible = false;

            //        if (DownloadLogButton != null)
            //            DownloadLogButton.Visible = false;

            //        ResultSummaryLabel.Text = "";
            //        TestResultLabel.Text = "";

            //        ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //        ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //        ResultSummaryLabel.Text += "<tr class='error'>" +
            //                        "<td>" + "gbXML schema Test" + "</td>" +
            //                        "<td>" + "You have not specified a right type of file." + "</td>" +
            //                        "<td>" + "Fail" + "</td>" +

            //                        "</tr>";
            //        ResultSummaryLabel.Text += "</table><br/>";
            //        #endregion Not XML File
            //    }
            //}
            ////if there is no file
            //else
            //{
            //    #region No File
            //    if (PrintFriendlyButton != null)
            //        PrintFriendlyButton.Visible = false;

            //    if (DownloadLogButton != null)
            //        DownloadLogButton.Visible = false;


            //    ResultSummaryLabel.Text = "";
            //    TestResultLabel.Text = "";

            //    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //    ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //    ResultSummaryLabel.Text += "<tr class='error'>" +
            //                    "<td>" + "gbXML schema Test" + "</td>" +
            //                    "<td>" + "You have not specified a file." + "</td>" +
            //                    "<td>" + "Fail" + "</td>" +
            //                    "</tr>";
            //    ResultSummaryLabel.Text += "</table><br/>";
            //    #endregion No File
            //}
        }
        protected void downloadJSONButton_Click(object sender, EventArgs e)
        {
            //if (FileUpload1.HasFile)
            //{
            //    if (FileUpload1.PostedFile.ContentType == "text/xml")
            //    {
            //        //if there is a file
            //        #region XML File
            //        //valadate it by pass in input stream as xmlreader
            //        Stream responseStream = FileUpload1.PostedFile.InputStream;
            //        XmlReader xmlreader = XmlReader.Create(responseStream);

            //        //validating xml
            //        DOEgbXMLValidator val = new DOEgbXMLValidator();
            //        //if it is not valid
            //        if (!val.IsValidXmlEx(xmlreader) || val.nErrors > 0 || val.nWarnings > 0)
            //        {
            //            if (PrintFriendlyButton != null)
            //                PrintFriendlyButton.Visible = false;

            //            if (DownloadLogButton != null)
            //                DownloadLogButton.Visible = false;

            //            //setup errorlog
            //            string errorLog = "";
            //            string errorDes = "";
            //            if (val.nErrors > 0 || val.nWarnings > 0)
            //            {
            //                errorLog += "<p class='text-error'><div class='alert alert-error'>" + "Find " + val.nErrors + " Errors and " + val.nWarnings + " Warnings <br/> <br/>" + val.Errors + "</div></p>";
            //                errorDes = "Find ";
            //                if (val.nErrors > 0)
            //                {
            //                    errorDes += val.nErrors;
            //                    if (val.nWarnings > 0)
            //                        errorDes += " Errors and";
            //                    else
            //                        errorDes += " Errors";

            //                }
            //                if (val.nWarnings > 0)
            //                    errorDes += val.nWarnings + " Warnings";
            //            }
            //            else
            //            {
            //                errorLog += "<p class='text-error'><div class='alert alert-error'>" + "Your XML File is severely deficient structurally.  It may be missing element tags or is not valid XML.  The test has failed. <br /><br/>" + val.BigError + "<br />" + "</div></p>";
            //                errorDes = "Your XML File is severely deficient structurally.";
            //            }
            //            // Session.Add("table", errorLog);
            //            Session["table"] = errorLog;

            //            TestResultLabel.Text = "";
            //        }
            //        //if it is valid
            //        else if (val.nErrors == 0 && val.nWarnings == 0)
            //        {
            //            //run test
            //            XMLParser parser = new XMLParser();

            //            responseStream.Position = 0;
            //            XmlReader xmlreader2 = XmlReader.Create(responseStream);

            //            /* Start Test */
            //            int jsonReport = 2;
            //            parser.StartReportDownloadTest(xmlreader2, "Test1", Page.User.Identity.Name, jsonReport);
            //        }
            //        //this should never happens
            //        else
            //        {
            //            ResultSummaryLabel.Text = "?????????something is very wrong";
            //            TestResultLabel.Text = "";
            //        }
            //        #endregion XML File
            //    }
            //    //if the file type is not xml
            //    else
            //    {
            //        #region Not XML File
            //        if (PrintFriendlyButton != null)
            //            PrintFriendlyButton.Visible = false;

            //        if (DownloadLogButton != null)
            //            DownloadLogButton.Visible = false;

            //        ResultSummaryLabel.Text = "";
            //        TestResultLabel.Text = "";

            //        ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //        ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //        ResultSummaryLabel.Text += "<tr class='error'>" +
            //                        "<td>" + "gbXML schema Test" + "</td>" +
            //                        "<td>" + "You have not specified a right type of file." + "</td>" +
            //                        "<td>" + "Fail" + "</td>" +

            //                        "</tr>";
            //        ResultSummaryLabel.Text += "</table><br/>";
            //        #endregion Not XML File
            //    }
            //}
            ////if there is no file
            //else
            //{
            //    #region No File
            //    if (PrintFriendlyButton != null)
            //        PrintFriendlyButton.Visible = false;

            //    if (DownloadLogButton != null)
            //        DownloadLogButton.Visible = false;


            //    ResultSummaryLabel.Text = "";
            //    TestResultLabel.Text = "";

            //    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //    ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //    ResultSummaryLabel.Text += "<tr class='error'>" +
            //                    "<td>" + "gbXML schema Test" + "</td>" +
            //                    "<td>" + "You have not specified a file." + "</td>" +
            //                    "<td>" + "Fail" + "</td>" +
            //                    "</tr>";
            //    ResultSummaryLabel.Text += "</table><br/>";
            //    #endregion No File
            //}
        }
        protected void importXMLButton_Click(object sender, EventArgs e)
        {
            //if (FileUpload1.HasFile)
            //{
            //    if (FileUpload1.PostedFile.ContentType == "text/xml")
            //    {
            //        //if there is a file
            //        #region XML File
            //        //valadate it by pass in input stream as xmlreader
            //        Stream responseStream = FileUpload1.PostedFile.InputStream;
            //        StreamReader reader = new StreamReader(responseStream);
            //        string xmlString = reader.ReadToEnd();
            //        try
            //        {
            //            XmlDocument document = new XmlDocument();
            //            document.LoadXml(xmlString);
            //            // Add a Schema to check against
            //            document.Schemas.Add(new XmlSchema());
            //            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
            //            document.Validate(eventHandler);

            //            //run test
            //            XMLParser parser = new XMLParser();

            //            responseStream.Position = 0;
            //            XmlReader xmlreader2 = XmlReader.Create(responseStream);
                        
            //            /* Start Import */
            //            dynamic report = parser.ImportXML(xmlreader2);
            //        }
            //        catch (Exception ex)
            //        {
            //            if (PrintFriendlyButton != null)
            //                PrintFriendlyButton.Visible = false;

            //            if (DownloadLogButton != null)
            //                DownloadLogButton.Visible = false;

            //            //setup errorlog
            //            string errorLog = "";
            //            errorLog += "<p class='text-error'><div class='alert alert-error'><p>" + ex + "</p></div></p>";
            //            Session["table"] = errorLog;
            //            TestResultLabel.Text = errorLog;
            //        }
            //        #endregion XML File
            //    }
            //    //if the file type is not xml
            //    else
            //    {
            //        #region Not XML File
            //        if (PrintFriendlyButton != null)
            //            PrintFriendlyButton.Visible = false;

            //        if (DownloadLogButton != null)
            //            DownloadLogButton.Visible = false;

            //        ResultSummaryLabel.Text = "";
            //        TestResultLabel.Text = "";

            //        ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //        ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //        ResultSummaryLabel.Text += "<tr class='error'>" +
            //                        "<td>" + "XML Test" + "</td>" +
            //                        "<td>" + "You have not specified a right type of file." + "</td>" +
            //                        "<td>" + "Fail" + "</td>" +

            //                        "</tr>";
            //        ResultSummaryLabel.Text += "</table><br/>";
            //        #endregion Not XML File
            //    }
            //}
            ////if there is no file
            //else
            //{
            //    #region No File
            //    if (PrintFriendlyButton != null)
            //        PrintFriendlyButton.Visible = false;

            //    if (DownloadLogButton != null)
            //        DownloadLogButton.Visible = false;


            //    ResultSummaryLabel.Text = "";
            //    TestResultLabel.Text = "";

            //    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //    ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //    ResultSummaryLabel.Text += "<tr class='error'>" +
            //                    "<td>" + "gbXML schema Test" + "</td>" +
            //                    "<td>" + "You have not specified a file." + "</td>" +
            //                    "<td>" + "Fail" + "</td>" +
            //                    "</tr>";
            //    ResultSummaryLabel.Text += "</table><br/>";
            //    #endregion No File
            //}
        }
        protected void importJSONButton_Click(object sender, EventArgs e)
        {
            //if (FileUpload1.HasFile)
            //{
            //    if (FileUpload1.PostedFile.ContentType == "application/octet-stream")
            //    {
            //        //if there is a file
            //        #region JSON File
            //        //valadate it by pass in input stream
            //        Stream responseStream = FileUpload1.PostedFile.InputStream;
            //        JsonTextReader jsonReader = new JsonTextReader(new StreamReader(responseStream));
            //        bool readable = true;
            //        //while (jsonReader.Read())
            //        //{
            //        //    if (jsonReader.Value == null)
            //        //    {
            //        //        readable = false;
            //        //    }
            //        //}
            //        //if it is not valid
            //        if (!readable)
            //        {
            //            if (PrintFriendlyButton != null)
            //                PrintFriendlyButton.Visible = false;

            //            if (DownloadLogButton != null)
            //                DownloadLogButton.Visible = false;

            //            //setup errorlog
            //            string errorLog = "";
            //            errorLog += "<p class='text-error'><div class='alert alert-error'>" + "Your JSON File is severely deficient structurally.  It may be missing element tags or is not valid JSON. The test has failed.</div></p>";
            //            Session["table"] = errorLog;
            //            TestResultLabel.Text = errorLog;
            //        }
            //        else
            //        {
            //            //run test
            //            XMLParser parser = new XMLParser();

            //            /* Start Import */
            //            parser.ImportJSON(jsonReader);
            //        }
            //        #endregion JSON File
            //    }
            //    //if the file type is not xml
            //    else
            //    {
            //        #region Not JSON File
            //        if (PrintFriendlyButton != null)
            //            PrintFriendlyButton.Visible = false;

            //        if (DownloadLogButton != null)
            //            DownloadLogButton.Visible = false;

            //        ResultSummaryLabel.Text = "";
            //        TestResultLabel.Text = "";

            //        ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //        ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //        ResultSummaryLabel.Text += "<tr class='error'>" +
            //                        "<td>" + "JSON Test" + "</td>" +
            //                        "<td>" + "You have not specified a right type of file." + "</td>" +
            //                        "<td>" + "Fail" + "</td>" +

            //                        "</tr>";
            //        ResultSummaryLabel.Text += "</table><br/>";
            //        #endregion Not JSON File
            //    }
            //}
            ////if there is no file
            //else
            //{
            //    #region No File
            //    if (PrintFriendlyButton != null)
            //        PrintFriendlyButton.Visible = false;

            //    if (DownloadLogButton != null)
            //        DownloadLogButton.Visible = false;


            //    ResultSummaryLabel.Text = "";
            //    TestResultLabel.Text = "";

            //    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
            //    ResultSummaryLabel.Text += "<table class='table table-bordered'>";
            //    ResultSummaryLabel.Text += "<tr class='error'>" +
            //                    "<td>" + "gbXML schema Test" + "</td>" +
            //                    "<td>" + "You have not specified a file." + "</td>" +
            //                    "<td>" + "Fail" + "</td>" +
            //                    "</tr>";
            //    ResultSummaryLabel.Text += "</table><br/>";
            //    #endregion No File
            //}
        }
        #endregion Test Buttons
        #region Download Report Buttons
        protected void DownloadLogButton_Click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();
            Response.AddHeader("content-disposition", "attachment;filename=Log.txt");
            Response.ContentType = "text/plain";
            Response.Write(LogLabel.Text);
            Response.End();
        }
        protected void PrintFriendlyButton_Click(object sender, EventArgs e)
        {
            Session.Add("table", TableLabel.Text);

            string url = "PrintFriendlyTablePage.aspx";

            ClientScript.RegisterStartupScript(this.GetType(), "OpenWindow", "<script>openNewWindow('" + url + "')</script>");
        }
        #endregion Download Report Buttons

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Console.WriteLine("Error: {0}", e.Message);
                    break;
                case XmlSeverityType.Warning:
                    Console.WriteLine("Warning {0}", e.Message);
                    break;
            }

        }
        private void ProcessValidXML(XMLParser parser, XmlReader xmlreader2)
        {
            #region Phase 1 Testing
            //start test
            //parser.StartTest(xmlreader2, "Test1", Page.User.Identity.Name);
            #endregion Phase 1 Testing
            #region Phase 2 Testing
            XmlDocument myxml = new XmlDocument();
            myxml.Load(xmlreader2);
            //3-get the namespace
            XmlNamespaceManager nsm = parser.getnsmanager(myxml);
            //figure out if metric or USIP (we have not found a reason to use this yet)
            parser.getunits(nsm, myxml);

            /* Begin Parsing the XML and reporting on it----------------------------------------------------------*/

            //make a reporting object
            DOEgbXMLPhase2Report report = new DOEgbXMLPhase2Report();

            /* Basic Uniqueness Constraints check-------------------------------------------------*/

            //Basic requirements check

            // Setup the results view for the log and web.
            log = "";
            table += "<div class='well-lg'>" +
                    "<h3>" + "Test Sections" + "</h3>";
            table += "<table class='table table-bordered'>";
            table += "<tr class='info'>" +
                                   "<td>" + "Test Section Name" + "</td>" +
                                   "<td>" + "Tolerances" + "</td>" +
                                   "<td>" + "Pass/Fail" + "</td>" +
                                   "</tr>";

            //first create a list of lists that is indexed identically to the drop down list the user selects
            TestDetail = new DOEgbXMLTestDetail();
            //then populate the list of lists.  All indexing is done "by hand" in InitializeTestResultStrings()
            TestDetail.InitializeTestResultStrings();                        //Set up the Global Pass/Fail criteria for the test case file
            TestCriteria = new DOEgbXMLTestCriteriaObject();
            TestCriteria.InitializeTestCriteriaWithTestName(TestToRun); // ("Test1");

            #region Reports
            //ensure that all names of spaces are unique--------------------------------------------------------------------------------------------------------Unique_Space_ID_Test//
            report.testType = TestType.Unique_Space_ID_Test;
            report = DOEgbXML.gbXMLSpaces.UniqueSpaceIdTest2(myxml, nsm, report);
            ProcessReport(report, true);
            report.Clear();

            //ensure that all space boundary names are unique--------------------------------------------------------------------------------------------------Unique_Space_Boundary//
            XmlNodeList nodes = myxml.SelectNodes("/gbXMLv5:gbXML/gbXMLv5:Campus/gbXMLv5:Building/gbXMLv5:Space/gbXMLv5:SpaceBoundary", nsm);
            if (nodes.Count > 0)
            {
                spaceBoundsPresent = true;
                report.testType = TestType.Unique_Space_Boundary;
                report = DOEgbXML.gbXMLSpaces.UniqueSpaceBoundaryIdTest2(myxml, nsm, report);
                ProcessReport(report, true);
                report.Clear();
            }
            else
            {
                //needs to be included so the report can be processed
                report.testType = TestType.Unique_Space_Boundary;
                report.passOrFail = true;
                report.longMsg = "A test is usually performed here to ensure Space Boundaries have valid naming conventions.  This test was skipped (legally) because your file does not have space boundaries present.  Continuing to next test.";
                ProcessReport(report, true);
                report.Clear();
            }

            //Space Tests
            //make a simplified representation of the spaces
            List<DOEgbXML.gbXMLSpaces> spaces = DOEgbXML.gbXMLSpaces.getSimpleSpaces(myxml, nsm);
            
            //4-check for self-intersecting polygons
            //report = DOEgbXML.gbXMLSpaces.SpaceSurfacesSelfIntersectionTest(spaces, report);
            //report.Clear();

            //check that all polyloops are in a counterclockwise direction-----------------------------------------------------------------------------------------Space_Surfaces_CC//
            report = DOEgbXML.gbXMLSpaces.SpaceSurfacesCCTest2(spaces, report);
            report.testType = TestType.Space_Surfaces_CC;
            ProcessReport(report, true);
            report.Clear();

            //check for non-planar objects for all Spaces' polyloops-------------------------------------------------------------------------------------------Space_Surfaces_Planar//
            report.coordtol = DOEgbXMLBasics.Tolerances.VectorAngleTolerance;
            report = DOEgbXML.gbXMLSpaces.SpaceSurfacesPlanarTest(spaces, report);
            report.testType = TestType.Space_Surfaces_Planar;
            ProcessReport(report, true);
            report.Clear();

            //valid space enclosure?---------------------------------------------------------------------------------------------------------------------------Check_Space_Enclosure//
            report.tolerance = 0.0001;
            //when we are comparing angles in this function, we are testing the angle between dot products
            report.vectorangletol = DOEgbXMLBasics.Tolerances.dotproducttol;
            report.lengthtol = DOEgbXMLBasics.Tolerances.lengthTolerance;
            //toler
            report.coordtol = DOEgbXMLBasics.Tolerances.coordToleranceIP;
            report = CheckSpaceEnclosureSG(spaces, report);
            report.testType = TestType.Check_Space_Enclosure;
            ProcessReport(report, true);
            report.Clear();

            /* Surface tests----------------------------------------------------------------------------------*/
            /* Basic Requirements ----------------------------------------------------------------------------*/

            //Are there at least 4 surface definitions?  (see the surface requirements at the campus node)-------------------------------------------------------At_Least_4_Surfaces//
            report.testType = TestType.At_Least_4_Surfaces;
            report = SurfaceDefinitions.AtLeast4Surfaces(myxml, nsm, report);
            ProcessReport(report, true);
            report.Clear();

            //Does the AdjacentSpaceId not exceed the max number allowable?-----------------------------------------------------------------------------------------Two_Adj_Space_Id//
            //this needs to be updated!
            report.testType = TestType.Two_Adj_Space_Id;
            report = SurfaceDefinitions.AtMost2SpaceAdjId(myxml, nsm, report);
            ProcessReport(report, true);
            report.Clear();

            //Are all required elements and attributes in place?---------------------------------------------------------------------------------------------Required_Surface_Fields//
            //report.testType = TestType.Required_Surface_Fields;
            //report = SurfaceDefinitions.RequiredSurfaceFields(myxml, nsm, report);
            //ProcessReport(report, true);
            //report.Clear();

            //ensure that all names of surfaces are unique------------------------------------------------------------------------------------------------------Surface_ID_Uniqueness//
            report.testType = TestType.Surface_ID_Uniqueness;
            report = DOEgbXML.SurfaceDefinitions.SurfaceIDUniquenessTest(myxml, nsm, report);
            ProcessReport(report, true);
            report.Clear();

            //now grab all the surfaceIds and make them available------------------------------------------------------------------------------------------------Surface_Adj_Id_Match//
            List<string> spaceIds = new List<string>();
            foreach (gbXMLSpaces s in spaces)
            {
                spaceIds.Add(s.id);
            }

            List<SurfaceDefinitions> surfaces = DOEgbXML.XMLParser.MakeSurfaceList(myxml, nsm);
            //make sure the surface Adjacent space Id names match only the the space Ids gathered above.  The adjacent space Ids can't have their own special values
            report.testType = TestType.Surface_Adj_Id_Match;
            report = DOEgbXML.SurfaceDefinitions.SurfaceAdjSpaceIdTest(spaceIds, surfaces, report);
            ProcessReport(report, true);
            report.Clear();

            //Surface_ID_SB_Match------------------------------------------------------------------------------------------------------------------------------Surface_ID_SB_Match//
            if (spaceBoundsPresent)
            {
                report.tolerance = DOEgbXMLBasics.Tolerances.coordToleranceIP;
                report.testType = TestType.Surface_ID_SB_Match;
                report = SurfaceDefinitions.SurfaceMatchesSpaceBoundary(myxml, nsm, report);
                ProcessReport(report, true);
                report.Clear();
            }
            else
            {
                report.testType = TestType.Surface_ID_SB_Match;
                report.passOrFail = true;
                report.longMsg = "A test is usually performed here to ensure Space Boundaries and Surfaces share the same ID.  This test was skipped (legally) because your file does not have space boundaries present.  Continuing to next test.";
                ProcessReport(report, true);
                report.Clear();
            }

            //Does the polyloop right hand rule vector form the proper azimuth and tilt? (with and without a CADModelAzimuth)----------------------------------Surface_Tilt_Az_Check//
            report.tolerance = DOEgbXMLBasics.Tolerances.VectorAngleTolerance;
            report.vectorangletol = DOEgbXMLBasics.Tolerances.VectorAngleTolerance;
            report.testType = TestType.Surface_Tilt_Az_Check;
            report = SurfaceDefinitions.SurfaceTiltAndAzCheck(myxml, nsm, report);
            ProcessReport(report, true);
            report.Clear();

            //planar surface test-------------------------------------------------------------------------------------------------------------------------------Surface_Planar_Test//
            report.vectorangletol = DOEgbXMLBasics.Tolerances.VectorAngleTolerance;
            report = SurfaceDefinitions.TestSurfacePlanarTest(surfaces, report);
            report.testType = TestType.Surface_Planar_Test;
            ProcessReport(report, true);
            report.Clear();

            //I must take the surfaces, group them, and rearrange any interior surfaces' coordinates that should be pointed the opposite way
            string searchpath = "/gbXMLv5:gbXML/gbXMLv5:Campus/gbXMLv5:Building/gbXMLv5:Space";
            List<string> spaceids = DOEgbXML.gbXMLSpaces.getSpaceIds(myxml, nsm, searchpath);
            Dictionary<string, List<SurfaceDefinitions>> enclosure = new Dictionary<string, List<SurfaceDefinitions>>();
            foreach (string id in spaceids)
            {
                //find all surfaces with this adjacent space id, get their polyloops, and then match their polyloops
                List<SurfaceDefinitions> surflist = new List<SurfaceDefinitions>();

                foreach (SurfaceDefinitions surf in surfaces)
                {
                    foreach (var adj in surf.AdjSpaceId)
                    {
                        if (adj == id)
                        {
                            surflist.Add(surf);
                            //don't want to add surfaces twice (slab on grade)
                            break;
                        }
                    }
                }
                enclosure[id] = surflist;
            }

            //counter clockwise winding test-------------------------------------------------------------------------------------------------------------------------Surface_CC_Test//
            report.testType = TestType.Surface_CC_Test;
            report = SurfaceDefinitions.SurfaceCCTest(enclosure, report);
            ProcessReport(report, true);
            report.Clear();

            //self intersecting polygon test
            //report = SurfaceDefinitions.SurfaceSelfIntersectionTest(surfaces, report);
            //report.Clear();

            //Is the Lower Left Corner properly defined?

            //surface enclosure tests------------------------------------------------------------------------------------------------------------------------Check_Surface_Enclosure//
            report.tolerance = 0.0001;
            report.vectorangletol = DOEgbXMLBasics.Tolerances.dotproducttol;
            report.lengthtol = DOEgbXMLBasics.Tolerances.lengthTolerance;
            report.coordtol = .01;
            report.testType = TestType.Check_Surface_Enclosure;
            report = CheckSurfaceEnclosure(enclosure, report);
            ProcessReport(report, true);
            report.Clear();
            #endregion Reports

            table += "</table></div>";
            CreateSummaryTable();
            #endregion Phase 2 Testing
        }
        private void ProcessReport(DOEgbXMLPhase2Report report, bool createTable)
        {
            //add report to report list
            //have to deep copy the report before put report in the list
            DOEgbXMLPhase2Report tmpreport = report.Copy();
            ReportList.Add(tmpreport);

            //title
            int subType = -1;
            string title = report.testType.ToString();
            if (report.subTestIndex != -1 && report.subTestIndex != null)
            {
                subType = report.subTestIndex;
            }
            title = title.Replace("_", " ");
            if (subType != -1)
            {
                title += " " + subType;
            }
            //message
            var passTest = report.TestPassedDict.Values;
            bool individualTestBool = true;
            foreach (bool testResult in passTest)
            {
                if (testResult == false)
                {
                    individualTestBool = false;
                    break;
                }
            }

            #region Create Log
            //description
            log += "Explanation of Test: " + report.testSummary + System.Environment.NewLine;

            if (report.passOrFail && individualTestBool)
            {
                log += "Test has Passed" + System.Environment.NewLine;
            }
            else
            {
                log += "Test has Failed" + System.Environment.NewLine;
                overallPassTest = false;
            }
            log += "Explanation of What Happened: " + report.longMsg + System.Environment.NewLine;

            //message list, print out each message in the list if there are any
            if (report.MessageList.Count() > 0)
            {
                foreach (KeyValuePair<string, List<string>> message in report.MessageList)
                {
                    foreach (string finding in message.Value)
                    {
                        log += message.Key + ": " + finding + System.Environment.NewLine;
                    }
                }
            }
            else if (report.TestPassedDict.Count() > 0)
            {
                foreach (KeyValuePair<string, bool> pair in report.TestPassedDict)
                {
                    log += pair.Key + ": " + pair.Value + System.Environment.NewLine;
                }
            }
            log += System.Environment.NewLine;
            #endregion Create Log

            //create table row
            if (createTable)
            {
                #region Create Table
                if (report.standResult.Count == 0)
                {
                    report.standResult.Add("---");
                    report.testResult.Add("---");
                    report.idList.Add("");
                }

                //for each output
                for (int i = 0; i < report.standResult.Count; i++)
                {
                    bool sameString = false;
                    if (report.standResult[i] == report.testResult[i])
                        sameString = true;

                    //check if test pass or fail
                    if ((report.passOrFail && individualTestBool))// || sameString)
                        table += "<tr class='success'>";
                    else
                    {
                        table += "<tr class='danger'>";
                        overallPassTest = false;
                    }

                    table += "<td style='text-align:left'>" + "<a href='TestDetailPage.aspx?type=" + (int)report.testType + "&subtype=" + report.subTestIndex + "' target='_blank'>" + title + " " + report.idList[i] + "</a>" + "</td>";

                    if ((report.passOrFail && individualTestBool))// || sameString)
                    {
                        table += "<td>" + "&plusmn" + report.tolerance + " " + report.unit + "</td>" +
                                "<td>Pass</td>" +
                                "</tr>";
                    }
                    else
                        table += "<td>" + "&plusmn" + report.tolerance + " " + report.unit + "</td>" +
                                "<td>Fail</td>" +
                                "</tr>";
                }
                #endregion Create Table
            }
        }
        private void CreateSummaryTable()
        {
            //create overall summary table
            //find the right testdetail
            //check if the user pass the test
            bool passTest = true;
            bool aceTest = true;
            foreach (DOEgbXMLPhase2Report tmpreport in ReportList)
            {
                if (TestCriteria.TestCriteriaDictionary.ContainsKey(tmpreport.testType))
                {
                    if (TestCriteria.TestCriteriaDictionary[tmpreport.testType] && !tmpreport.passOrFail)
                        passTest = false;
                    if (!TestCriteria.TestCriteriaDictionary[tmpreport.testType] && !tmpreport.passOrFail)
                        aceTest = false;
                }
                else if (tmpreport.testType == TestType.Detailed_Surface_Checks)
                {

                }
                else
                {

                }
            }
            foreach (DOEgbXMLTestDetail detail in TestDetail.TestDetailList)
            {
                // TODO: Set to "TestToRun" after "GenericPhase2" is added to DOEgbXMLTestDetail.
                if (detail.testName == "Test1") //== TestToRun)
                {
                    summaryTable = "<h3>Result Summary</h3>";
                    summaryTable += "<div class='well-sm'><table class='table table-bordered'>";

                    summaryTable += "<tr class='success'>" +
                                    "<td>" + "gbXML schema Test" + "</td>" +
                                    "<td>" + "" + "</td>" +
                                    "<td>" + "Pass" + "</td>" +
                                    "</tr>";

                    if (passTest && aceTest)
                        summaryTable += "<tr class='success'>";
                    else if (passTest)
                        summaryTable += "<tr class='warning'>";
                    else
                        summaryTable += "<tr class='danger'>";

                    summaryTable += "<td>" + "gbXML Test" + "</td>" +
                                    "<td>" + detail.shortTitle + "</td>";

                    if (passTest && aceTest)
                        summaryTable += "<td>" + detail.passString + "</td>" + "</tr>";
                    else if (passTest)
                        summaryTable += "<td>" + "You pass the test with minor errors" + "</td>" + "</tr>";
                    else
                        summaryTable += "<td>" + detail.failString + "</td>" + "</tr>";

                    summaryTable += "</table></div><br/>";
                    break;
                }
            }
        }

        public static DOEgbXMLPhase2Report CheckSpaceEnclosureSG(List<gbXMLSpaces> spaces, DOEgbXMLPhase2Report report)
        {
            try
            {
                report.testSummary = "This test checks the enclosure defined by the ShellGeometry PolyLoops for each given space in your gbXML file.";
                report.testSummary += " This is an optional test because ShellGeometry definitions are optional.";
                report.testSummary += " An enclosure test is important because it ensures that each of the surfaces in the gbXML definition is properly aligned ";
                report.testSummary += " with its neighbor.  The test checks to make sure that all edges of each surface line up with one another so that there are not";
                report.testSummary += "any gaps.";
                report.passOrFail = true;
                foreach (gbXMLSpaces space in spaces)
                {
                    List<string> ml = new List<string>();
                    Dictionary<int, Vector.EdgeFamily> uniqueedges = new Dictionary<int, Vector.EdgeFamily>();

                    if (space.sg.cs.ploops.Count() > 0)
                    {
                        string rep = space.id + " has ShellGeometry PolyLoops.  Conducting tests of ShellGeometry PolyLoops water tightness";
                        report.TestPassedDict[rep] = true;
                        int sgcount = 1;
                        foreach (DOEgbXML.gbXMLSpaces.PolyLoop pl in space.sg.cs.ploops)
                        {
                            string surfaceid = "shellgeometry-" + sgcount;
                            uniqueedges = Vector.GetEdgeFamilies(surfaceid, uniqueedges, pl.plcoords, report.coordtol, report.vectorangletol);
                            sgcount++;
                        }
                        if (uniqueedges.Count > 0)
                        {
                            string erep = space.id + ": Gathered edges of the ShellGeometry PolyLoop successfullly.";
                            report.TestPassedDict[erep] = true;
                        }
                        else
                        {
                            string erep = space.id + ": Gathered edges of the ShellGeometry PolyLoop unsuccessfullly.";
                            report.TestPassedDict[erep] = false;
                        }

                        //see how well enclosure is formed
                        //new function added April 11, 2014
                        report = MatchEdges(uniqueedges, ml, report, space.id);
                    }
                    else
                    {
                        string rep = space.id + " does not has ShellGeometry PolyLoops (this is not an error).  Conducting tests of ShellGeometry PolyLoops water tightness";
                        report.TestPassedDict[rep] = false;
                    }
                    report.MessageList[space.id] = ml;
                }

            }
            catch (Exception e)
            {
                report.longMsg = ("SORRY, we have run into an unexpected issue:" + e.ToString());
                report.passOrFail = false;
                return report;
            }
            return report;
        }
        public static DOEgbXMLPhase2Report CheckSurfaceEnclosure(Dictionary<string, List<SurfaceDefinitions>> surfaceEnclosures, DOEgbXMLPhase2Report report)
        {
            try
            {
                report.testSummary = "This test checks surfaces proclaiming to be children of a given space ID.";
                report.testSummary += "  It searches each of the surfaces' edges and tries to find other edges that align.";
                report.passOrFail = true;
                foreach (KeyValuePair<string, List<SurfaceDefinitions>> kp in surfaceEnclosures)
                {
                    List<string> ml = new List<string>();
                    ml.Add(kp.Key + ": Testing begins.");
                    Dictionary<int, Vector.EdgeFamily> uniqueedges = new Dictionary<int, Vector.EdgeFamily>();

                    foreach (SurfaceDefinitions surface in kp.Value)
                    {
                        uniqueedges = Vector.GetEdgeFamilies(surface.SurfaceId, uniqueedges, surface.PlCoords, report.coordtol, report.vectorangletol);
                    }
                    ml.Add("Gathered edges and their neighboring relationships.");
                    ml.Add("Validating the surfaces' edges alignment with one another (water tightness check.");
                    //check the edge families to see how water tight the edges are
                    //there should always be at least one related Edge
                    //if there is only one, it should match exactly (or within some settable tolerance)
                    //if there is more than one, they each should only intersect at their ends (within some tolerance) and not intersect
                    //and there should be no gaps 
                    //new function added April 11, 2014
                    report = MatchEdges(uniqueedges, ml, report, kp.Key);

                    report.MessageList[kp.Key] = ml;
                }
            }
            catch (Exception e)
            {

            }
            return report;
        }
        public static DOEgbXMLPhase2Report MatchEdges(Dictionary<int, Vector.EdgeFamily> uniqueedges, List<string> ml, DOEgbXMLPhase2Report report, string spaceid)
        {
            try
            {
                int totaledgect = 0;
                int matchededges = 0;
                string lastedgenm = "";
                int surfedgect = 0;
                foreach (KeyValuePair<int, Vector.EdgeFamily> edgekp in uniqueedges)
                {
                    //a way to count edges
                    if (edgekp.Value.sbdec != lastedgenm)
                    {
                        //reset
                        lastedgenm = edgekp.Value.sbdec;
                        surfedgect = 0;
                    }
                    //here is the easiest case where there is only one related edge
                    //we know this must be a perfect match, or entirely envelopes the edge 
                    if (edgekp.Value.relatedEdges.Count() == 1)
                    {
                        Vector.MemorySafe_CartCoord edgestart = edgekp.Value.startendpt[0];
                        Vector.MemorySafe_CartCoord edgeend = edgekp.Value.startendpt[1];
                        Vector.MemorySafe_CartCoord relstart = edgekp.Value.relatedEdges[0].startendpt[0];
                        Vector.MemorySafe_CartCoord relend = edgekp.Value.relatedEdges[0].startendpt[1];
                        //if the lengths are the same, then they should match perfectly.
                        //this is a valid conclusion because we already have identified that they aligh and 
                        //are in the same space.
                        double edgeX = edgestart.X - edgeend.X;
                        double edgeY = edgestart.Y - edgeend.Y;
                        double edgeZ = edgestart.Z - edgeend.Z;
                        Vector.MemorySafe_CartVect edgev = new Vector.MemorySafe_CartVect(edgeX, edgeY, edgeZ);
                        double edgemag = Vector.VectorMagnitude(edgev);

                        double relX = relstart.X - relend.X;
                        double relY = relstart.Y - relend.Y;
                        double relZ = relstart.Z - relend.Z;
                        Vector.MemorySafe_CartVect relv = new Vector.MemorySafe_CartVect(relX, relY, relZ);
                        double relmag = Vector.VectorMagnitude(relv);
                        //do the check here to see if the two edges (current and related) are the same length
                        if (Math.Abs(relmag - edgemag) < report.coordtol)
                        {

                            //should match perfectly
                            ml.Add(edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " should have perfectly matched coordinates.");
                            List<bool> match = new List<bool>();
                            double tol = report.tolerance;
                            for (int i = 0; i < 2; i++)
                            {
                                Vector.MemorySafe_CartCoord p1 = edgekp.Value.relatedEdges[0].startendpt[i];
                                for (int j = 0; j < 2; j++)
                                {
                                    string x = p1.X.ToString();
                                    string y = p1.Y.ToString();
                                    string z = p1.Z.ToString();
                                    string coordstr = "(" + x + "," + y + "," + z + ")";
                                    Vector.MemorySafe_CartCoord p2 = edgekp.Value.startendpt[j];
                                    if (p2.X == p1.X && p2.Y == p1.Y && p2.Z == p1.Z)
                                    {
                                        match.Add(true);
                                        ml.Add("PERFECT MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                    }
                                    else if (Math.Abs(p2.X - p1.X) < tol && Math.Abs(p2.Y - p1.Y) < report.coordtol && Math.Abs(p2.Z - p1.Z) < report.coordtol)
                                    {
                                        match.Add(true);
                                        ml.Add("MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                        report.passOrFail = false;
                                    }
                                }
                            }
                            if (match.Count() == 2)
                            {
                                ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " forms a tight enclosure with its neighbor.");
                                report.passOrFail = true;
                                //we +2 here because the related edge is not recorded
                                totaledgect += 2;
                                matchededges += 2;
                            }
                            else
                            {
                                ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " does not form a tight enclosure with its neighbor.");
                                report.passOrFail = false;
                                //we +2 here because the related edge is not recorded
                                totaledgect += 2;
                                matchededges += 0;
                            }
                        }
                        //April 7 2014
                        //it is safe to conclude that the the two are related, but they overlap.  In this case, since there is only one neighbor
                        //it should be the case that one edge entirely envelops the other edge.  
                        //this edge, has to be the one that is enveloped, because it only has one related edge, by convention
                        else
                        {
                            ml.Add(edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " should be enclosed by its neighboring edge.");
                            //es--------------ee
                            //rs-------------------re
                            if (Math.Abs(edgestart.X - relstart.X) <= report.coordtol && Math.Abs(edgestart.Y - relstart.Y) <= report.coordtol && Math.Abs(edgestart.Z - relstart.Z) <= coordtol)
                            {
                                string x = edgestart.X.ToString();
                                string y = edgestart.Y.ToString();
                                string z = edgestart.Z.ToString();
                                string coordstr = "(" + x + "," + y + "," + z + ")";
                                ml.Add("MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                double Cx = edgeend.X - relstart.X;
                                double Cy = edgeend.Y - relstart.Y;
                                double Cz = edgeend.Z - relstart.Z;
                                Vector.MemorySafe_CartVect C = new Vector.MemorySafe_CartVect(Cx, Cy, Cz);

                                double Dx = edgeend.X - relend.X;
                                double Dy = edgeend.Y - relend.Y;
                                double Dz = edgeend.Z - relend.Z;
                                Vector.MemorySafe_CartVect D = new Vector.MemorySafe_CartVect(Dx, Dy, Dz);

                                double dotend = Vector.DotProductMag(C, D);
                                //both of these dot products should point in opposite directions, proving the edge is entirely enveloped
                                string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                                string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                                if (Math.Abs(dotend) - 1 <= report.vectorangletol)
                                {
                                    ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate "+coord + " and " + coord2 +" forms a tight enclosure with its overlapping neighbor.");
                                }
                                else
                                {
                                    ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + "does not form a tight enclosure with its overlapping neighbor.");
                                    report.passOrFail = false;
                                }
                            }
                            //es-----------------ee
                            //re--------------------------rs
                            else if (Math.Abs(edgestart.X - relend.X) <= report.coordtol && Math.Abs(edgestart.Y - relend.Y) <= report.coordtol && Math.Abs(edgestart.Z - relend.Z) <= coordtol)
                            {
                                string x = edgestart.X.ToString();
                                string y = edgestart.Y.ToString();
                                string z = edgestart.Z.ToString();
                                string coordstr = "(" + x + "," + y + "," + z + ")";
                                ml.Add("MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                double Cx = edgeend.X - relstart.X;
                                double Cy = edgeend.Y - relstart.Y;
                                double Cz = edgeend.Z - relstart.Z;
                                Vector.MemorySafe_CartVect C = new Vector.MemorySafe_CartVect(Cx, Cy, Cz);

                                double Dx = edgeend.X - relend.X;
                                double Dy = edgeend.Y - relend.Y;
                                double Dz = edgeend.Z - relend.Z;
                                Vector.MemorySafe_CartVect D = new Vector.MemorySafe_CartVect(Dx, Dy, Dz);

                                double dotend = Vector.DotProductMag(C, D);
                                //both of these dot products should point in opposite directions, proving the edge is entirely enveloped
                                string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                                string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                                if (Math.Abs(dotend) - 1 <= report.vectorangletol)
                                {
                                    ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + " forms a tight enclosure with its overlapping neighbor.");
                                    totaledgect += 1;
                                    matchededges += 1;
                                }
                                else
                                {
                                    ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + "with Coordinate " + coord + " and " + coord2 + "does not form a tight enclosure with its overlapping neighbor.");
                                    report.passOrFail = false;
                                    totaledgect += 1;
                                    matchededges += 0;
                                }
                            }
                            //ee-----------------es
                            //rs--------------------------re
                            else if (Math.Abs(edgeend.X - relstart.X) <= report.coordtol && Math.Abs(edgeend.Y - relstart.Y) <= report.coordtol && Math.Abs(edgeend.Z - relstart.Z) <= coordtol)
                            {
                                string x = edgeend.X.ToString();
                                string y = edgeend.Y.ToString();
                                string z = edgeend.Z.ToString();
                                string coordstr = "(" + x + "," + y + "," + z + ")";
                                ml.Add("MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                double Ax = edgestart.X - relstart.X;
                                double Ay = edgestart.Y - relstart.Y;
                                double Az = edgestart.Z - relstart.Z;
                                Vector.MemorySafe_CartVect A = new Vector.MemorySafe_CartVect(Ax, Ay, Az);

                                double Bx = edgestart.X - relend.X;
                                double By = edgestart.Y - relend.Y;
                                double Bz = edgestart.Z - relend.Z;
                                Vector.MemorySafe_CartVect B = new Vector.MemorySafe_CartVect(Bx, By, Bz);

                                double dotstart = Vector.DotProductMag(A, B);
                                //both of these dot products should point in opposite directions, proving the edge is entirely enveloped
                                string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                                string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                                if (Math.Abs(dotstart) - 1 <= report.vectorangletol)
                                {
                                    ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + " forms a tight enclosure with its overlapping neighbor.");
                                    totaledgect += 1;
                                    matchededges += 1;
                                }
                                else
                                {
                                    ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + "with Coordinate " + coord + " and " + coord2 + "does not form a tight enclosure with its overlapping neighbor.");
                                    report.passOrFail = false;
                                    totaledgect += 1;
                                    matchededges += 0;
                                }
                            }
                            //ee-----------------es
                            //re--------------------------rs
                            else if (Math.Abs(edgeend.X - relend.X) <= report.coordtol && Math.Abs(edgeend.Y - relend.Y) <= report.coordtol && Math.Abs(edgeend.Z - relend.Z) <= coordtol)
                            {
                                string x = edgeend.X.ToString();
                                string y = edgeend.Y.ToString();
                                string z = edgeend.Z.ToString();
                                string coordstr = "(" + x + "," + y + "," + z + ")";
                                ml.Add("MATCH: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " Coordinate " + coordstr);
                                double Ax = edgestart.X - relstart.X;
                                double Ay = edgestart.Y - relstart.Y;
                                double Az = edgestart.Z - relstart.Z;
                                Vector.MemorySafe_CartVect A = new Vector.MemorySafe_CartVect(Ax, Ay, Az);

                                double Bx = edgestart.X - relend.X;
                                double By = edgestart.Y - relend.Y;
                                double Bz = edgestart.Z - relend.Z;
                                Vector.MemorySafe_CartVect B = new Vector.MemorySafe_CartVect(Bx, By, Bz);

                                double dotstart = Vector.DotProductMag(A, B);
                                //both of these dot products should point in opposite directions, proving the edge is entirely enveloped
                                string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                                string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                                if (Math.Abs(dotstart) - 1 <= report.vectorangletol)
                                {
                                    ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + " forms a tight enclosure with its overlapping neighbor.");
                                    totaledgect += 1;
                                    matchededges += 1;
                                }
                                else
                                {
                                    ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + "with Coordinate " + coord + " and " + coord2 + "does not form a tight enclosure with its overlapping neighbor.");
                                    report.passOrFail = false;
                                    totaledgect += 1;
                                    matchededges += 0;
                                }
                            }
                            //overlapping edges
                            else
                            {
                                ml.Add(edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " is overlapped at both ends by its neighboring edge.");
                                double Ax = edgestart.X - relstart.X;
                                double Ay = edgestart.Y - relstart.Y;
                                double Az = edgestart.Z - relstart.Z;
                                Vector.MemorySafe_CartVect A = new Vector.MemorySafe_CartVect(Ax, Ay, Az);

                                double Bx = edgestart.X - relend.X;
                                double By = edgestart.Y - relend.Y;
                                double Bz = edgestart.Z - relend.Z;
                                Vector.MemorySafe_CartVect B = new Vector.MemorySafe_CartVect(Bx, By, Bz);

                                double Cx = edgeend.X - relstart.X;
                                double Cy = edgeend.Y - relstart.Y;
                                double Cz = edgeend.Z - relstart.Z;
                                Vector.MemorySafe_CartVect C = new Vector.MemorySafe_CartVect(Cx, Cy, Cz);

                                double Dx = edgeend.X - relend.X;
                                double Dy = edgeend.Y - relend.Y;
                                double Dz = edgeend.Z - relend.Z;
                                Vector.MemorySafe_CartVect D = new Vector.MemorySafe_CartVect(Dx, Dy, Dz);

                                double dotstart = Vector.DotProductMag(A, B);
                                double dotend = Vector.DotProductMag(C, D);
                                //both of these dot products should point in opposite directions, proving the edge is entirely enveloped
                                string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                                string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                                if (Math.Abs(dotstart) - 1 <= report.vectorangletol && Math.Abs(dotend) - 1 <= report.vectorangletol)
                                {
                                    ml.Add("PASS: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + " forms a tight enclosure with its overlapping neighbor.");
                                    totaledgect += 1;
                                    matchededges += 1;
                                }
                                else
                                {
                                    ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + "with Coordinate " + coord + " and " + coord2 + "does not form a tight enclosure with its overlapping neighbor.");
                                    report.passOrFail = false;
                                    totaledgect += 1;
                                    matchededges += 0;
                                }
                            }
                        }

                    }
                    else if (edgekp.Value.relatedEdges.Count() > 1)
                    {
                        ml.Add(edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " has " + edgekp.Value.relatedEdges.Count() + " neighboring edges.");
                        //more robust testing
                        Vector.EdgeFamily[] orderededges = new Vector.EdgeFamily[edgekp.Value.relatedEdges.Count()];
                        //align the related edges
                        orderededges = AlignEdges(orderededges, edgekp.Value, coordtol);

                        int coordcount = 0;
                        double segmentslength = 0;
                        int lastct = edgekp.Value.relatedEdges.Count() * 2 - 2;
                        Vector.CartCoord st = new Vector.CartCoord();
                        Vector.CartCoord end = new Vector.CartCoord();
                        foreach (Vector.EdgeFamily edge in orderededges)
                        {

                            if (coordcount == 0)
                            {
                                st.X = edge.startendpt[0].X;
                                st.Y = edge.startendpt[0].Y;
                                st.Z = edge.startendpt[0].Z;
                            }

                            if (coordcount == lastct)
                            {
                                end.X = edge.startendpt[1].X;
                                end.Y = edge.startendpt[1].Y;
                                end.Z = edge.startendpt[1].Z;
                            }
                            Vector.MemorySafe_CartVect v = Vector.CreateMemorySafe_Vector(edge.startendpt[0], edge.startendpt[1]);
                            double mag = Vector.VectorMagnitude(v);
                            segmentslength += mag;
                            coordcount += 2;
                        }
                        Vector.MemorySafe_CartVect v2 = Vector.CreateMemorySafe_Vector(edgekp.Value.startendpt[0], edgekp.Value.startendpt[1]);
                        double mag2 = Vector.VectorMagnitude(v2);
                        string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                        string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                        if (Math.Abs(segmentslength - mag2) < report.lengthtol)
                        {
                            ml.Add("PASS: " + edgekp.Value.sbdec + " with Coordinate " + coord + " and " + coord2 + " Multiple Overlapping edges properly match.");
                        }
                        else
                        {
                            //then something is wrong.
                            ml.Add("FAIL: " + edgekp.Value.sbdec + " with Coordinate " + coord + " and " + coord2 + " Overlapping edges do not match as expected.");
                            report.passOrFail = false;
                        }

                    }
                    else if (edgekp.Value.relatedEdges.Count() == 0)
                    {
                        //something is wrong
                        string coord = "(" + edgekp.Value.startendpt[0].X + "," + edgekp.Value.startendpt[0].Y + "," + edgekp.Value.startendpt[0].Z + ")";
                        string coord2 = "(" + edgekp.Value.startendpt[1].X + "," + edgekp.Value.startendpt[1].Y + "," + edgekp.Value.startendpt[1].Z + ")";
                        ml.Add("FAIL: " + edgekp.Value.sbdec + " Edge " + surfedgect.ToString() + " with Coordinate " + coord + " and " + coord2 + " has no reported neighboring edges.");
                        report.passOrFail = false;
                    }
                    surfedgect += 1;
                }
                report.MessageList[spaceid] = ml;
                //all edges align = true
                if (report.passOrFail)
                {
                    report.longMsg = "TEST PASSED: " + totaledgect + " edges in the gbXML file.  " + matchededges + " edges found with ideal alignment.";
                }
                //all edges align = false
                else
                {
                    report.longMsg = "TEST FAILED: " + totaledgect + " edges in the gbXML file.  " + matchededges + " edges found with ideal alignment.";
                }
                return report;

            }
            catch (Exception e)
            {
                report.longMsg = ("ERROR, we have run into an unexpected issue:" + e.ToString());
                report.passOrFail = false;
                return report;
            }

        }
        public static Vector.EdgeFamily[] AlignEdges(Vector.EdgeFamily[] alignededges, Vector.EdgeFamily edge, double tol)
        {
            try
            {
                List<double> magnitudes = new List<double>();
                List<int> indices = new List<int>();
                for (int i = 0; i < edge.relatedEdges.Count(); i++)
                {
                    Vector.MemorySafe_CartCoord longstart = edge.startendpt[0];
                    Vector.MemorySafe_CartCoord longend = edge.startendpt[1];
                    Vector.MemorySafe_CartVect longv = Vector.CreateMemorySafe_Vector(longstart, longend);
                    double maglong = Vector.VectorMagnitude(longv);
                    Vector.EdgeFamily shortedge = edge.relatedEdges[i];
                    Vector.MemorySafe_CartCoord shortstart = shortedge.startendpt[0];
                    Vector.MemorySafe_CartCoord shortend = shortedge.startendpt[1];
                    Vector.MemorySafe_CartVect el1 = Vector.CreateMemorySafe_Vector(longstart, shortstart);
                    Vector.MemorySafe_CartVect el2 = Vector.CreateMemorySafe_Vector(longstart, shortend);
                    double magel1 = Vector.VectorMagnitude(el1);
                    double magel2 = Vector.VectorMagnitude(el2);


                    //put the greater of the two magnitudes in the list
                    if (magel1 > magel2)
                    {
                        shortedge.startendpt.Reverse();
                        if (magnitudes.Count >= 1)
                        {
                            bool added = false;
                            for (int m = 0; m < magnitudes.Count(); m++)
                            {
                                if (magel1 < magnitudes[m])
                                {
                                    magnitudes.Insert(m, magel1);
                                    indices.Insert(m, i);
                                    added = true;
                                    break;
                                }
                            }
                            if (!added)
                            {
                                magnitudes.Add(magel1);
                                indices.Add(i);
                            }

                        }
                        else
                        {
                            magnitudes.Add(magel1);
                            indices.Add(i);
                        }
                    }
                    else
                    {
                        if (magnitudes.Count >= 1)
                        {
                            bool added = false;
                            for (int m = 0; m < magnitudes.Count(); m++)
                            {
                                if (magel1 < magnitudes[m])
                                {
                                    magnitudes.Insert(m, magel2);
                                    indices.Insert(m, i);
                                    added = true;
                                    break;
                                }
                            }
                            if (!added)
                            {
                                magnitudes.Add(magel2);
                                indices.Add(i);
                            }

                        }
                        else
                        {
                            magnitudes.Add(magel2);
                            indices.Add(i);
                        }
                    }

                }
                int alignedcounter = 0;
                foreach (int i in indices)
                {
                    alignededges[alignedcounter] = edge.relatedEdges[i];
                    alignedcounter++;
                }

                return alignededges;


            }
            catch (Exception e)
            {

            }
            return alignededges;
        }
    }
}