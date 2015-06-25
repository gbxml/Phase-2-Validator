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
        //public string table;  // TODO: Replace "table" string with outputs to a panel???
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
            TestSummuryPanel.Controls.Add(new LiteralControl("<li>Level 3: Checks for valid water tight geometry.</li>"));
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
                        if (PrintFriendly != null)
                            PrintFriendly.Visible = false;

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

                        TestResultPanel.Controls.Clear();
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
                    }
                    else if (val.nErrors == 0 && val.nWarnings == 0)
                    {
                        //if it is valid
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
                        //TestResultLabel.Text = table;
                        
                        //store reportlist in session for TestDetailPage.
                        Session["reportList"] = ReportList;
 
                        LogLabel.Text = log;
                        //TableLabel.Text = table;
                        //remove extra tag
                        TableLabel.Text = TableLabel.Text.Replace("<a href='PrintFriendlyTablePage.aspx' target='_blank'>", "");
                        TableLabel.Text = TableLabel.Text.Replace("</a>", "");
                        DownloadLogButton.Visible = true;
                        PrintFriendly.Visible = true;
                    }
                    //this should never happen
                    else
                    {
                        ResultSummaryLabel.Text = "?????????something is very wrong";
                        TestResultPanel.Controls.Clear();
                    }
                }
                //if the file type is not xml
                else
                {
                    if (PrintFriendly != null)
                        PrintFriendly.Visible = false;

                    if (DownloadLogButton != null)
                        DownloadLogButton.Visible = false;

                    ResultSummaryLabel.Text = "";
                    TestResultPanel.Controls.Clear();
                    ResultSummaryLabel.Text = "<h3>Result Summary</h3>";
                    ResultSummaryLabel.Text += "<table class='table'>";
                    ResultSummaryLabel.Text += "<tr class='alert alert-danger'>" +
                                    "<td>" + "gbXML schema Test" + "</td>" +
                                    "<td>" + "You have not specified a right type of file." + "</td>" +
                                    "<td>" + "Fail" + "</td>" +
                                    "</tr>";
                    ResultSummaryLabel.Text += "</table><br/>";
                    ResultsSections.Visible = true;
                }
            }
            //if there is no file
            else
            {
                if (PrintFriendly != null)
                    PrintFriendly.Visible = false;

                if (DownloadLogButton != null)
                    DownloadLogButton.Visible = false;

                ResultSummaryLabel.Text = "";
                TestResultPanel.Controls.Clear();
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

        // Download Report Buttons
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
            // Phase 2 Testing
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
            TestResultPanel.Controls.Add(new LiteralControl("<div class='well-lg'><h3>Test Sections</h3><table class='table table-bordered'><tr class='info'><td>Test Section Name</td><td>Tolerances</td><td>Pass/Fail</td></tr>"));

            //first create a list of lists that is indexed identically to the drop down list the user selects
            TestDetail = new DOEgbXMLTestDetail();
            //then populate the list of lists.  All indexing is done "by hand" in InitializeTestResultStrings()
            TestDetail.InitializeTestResultStrings();                        //Set up the Global Pass/Fail criteria for the test case file
            TestCriteria = new DOEgbXMLTestCriteriaObject();
            TestCriteria.InitializeTestCriteriaWithTestName(TestToRun); // ("Test1");

            // Reports
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
            report = Validator.CheckSpaceEnclosureSG(spaces, report);
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
            report = Validator.CheckSurfaceEnclosure(enclosure, report);
            ProcessReport(report, true);
            report.Clear();

            TestResultPanel.Controls.Add(new LiteralControl("</table></div>"));
            CreateSummaryTable();
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

            //create table row
            if (createTable)
            {
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
                        TestResultPanel.Controls.Add(new LiteralControl("<tr class='success'>"));
                    else
                    {
                        TestResultPanel.Controls.Add(new LiteralControl("<tr class='danger'>"));
                        overallPassTest = false;
                    }

                    TestResultPanel.Controls.Add(new LiteralControl("<td style='text-align:left'><a href='TestDetailPage.aspx?type=" + (int)report.testType + "&subtype=" + report.subTestIndex + "' target='_blank'>" + title + " " + report.idList[i] + "</a></td>"));

                    if ((report.passOrFail && individualTestBool))// || sameString)
                    {
                        TestResultPanel.Controls.Add(new LiteralControl("<td>&plusmn" + report.tolerance + " " + report.unit + "</td><td>Pass</td></tr>"));
                    }
                    else
                    {
                        TestResultPanel.Controls.Add(new LiteralControl("<td>&plusmn" + report.tolerance + " " + report.unit + "</td><td>Fail</td></tr>"));
                    }
                }
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
    }
}