#region Assembly DOEgbxml.dll, v4.0.30319
// C:\Users\CarmelDeveloper\SoftwareDevelopment\C_Sharp\Applications\gbXMLValidator\gbXMLValidator\bin\Debug\DOEgbxml.dll
#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using VectorMath;

namespace DOEgbXML
{
    public class XMLParserNew
    {
        public string log;
        public string output;
        public List<DOEgbXMLReportingObj> ReportList;
        public string summaryTable;
        public string table;

        public XMLParserNew();

        public static DOEgbXMLReportingObj GetAirSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetBuildingArea(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetBuildingSpaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetBuildingStoryCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetEWSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static List<SurfaceDefinitions> GetFileSurfaceDefs(XmlDocument xmldoc, XmlNamespaceManager xmlns);
        public static DOEgbXMLReportingObj GetIFSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetIWSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public XmlNamespaceManager getnsmanager(XmlDocument xmldoc);
        public static Vector.CartVect GetPolyLoopXProduct(XmlNodeList PlanarGeometry, string level, TestType testType);
        public static DOEgbXMLReportingObj GetRoofSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetShadeSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static Dictionary<string, List<Vector.CartVect>> GetShellGeomPolyRHR(Dictionary<string, List<Vector.CartCoord>> PtsList);
        public static Dictionary<string, List<Vector.CartCoord>> GetShellGeomPts(XmlNode closedShell);
        public static DOEgbXMLReportingObj GetStoryHeights(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj GetSurfaceCount(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public int getunits(XmlNamespaceManager xmlns, XmlDocument xmldoc);
        public int LoadFile(XmlReader xr, XmlDocument doc);
        public static List<SurfaceDefinitions> MakeSurfaceList(XmlDocument xmldoc, XmlNamespaceManager xmlns);
        public void StartTest(XmlReader xmldoc, string testToRun, string username);
        public static DOEgbXMLReportingObj TestBuildingStoryRHR(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj TestShellGeomPLRHR(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj TestSpaceAreas(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj TestSpaceVolumes(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report, string Units);
        public static DOEgbXMLReportingObj UniqueSpaceIdTest(List<XmlDocument> gbXMLDocs, List<XmlNamespaceManager> gbXMLnsm, DOEgbXMLReportingObj report);
    }
}
