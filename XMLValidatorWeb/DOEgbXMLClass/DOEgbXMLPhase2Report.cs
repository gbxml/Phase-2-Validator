#region Assembly DOEgbxml.dll, v4.0.30319
// C:\Users\CarmelDeveloper\SoftwareDevelopment\C_Sharp\Applications\gbXMLValidator\gbXMLValidator\bin\Debug\DOEgbxml.dll
#endregion

using System;
using System.Collections.Generic;

namespace DOEgbXML
{
    public class DOEgbXMLPhase2Report
    {
        public double coordtol;
        public Exception e;
        public List<string> idList;
        public double lengthtol;
        public string longMsg;
        public Dictionary<string, List<string>> MessageList;
        public bool passOrFail;
        public List<string> standResult;
        public int subTestIndex;
        public Dictionary<string, bool> TestPassedDict;
        public string testReasoning;
        public List<string> testResult;
        public string testSummary;
        public TestType testType;
        public double tolerance;
        public string unit;
        public double vectorangletol;

        public DOEgbXMLPhase2Report();

        public void Clear();
        public DOEgbXMLPhase2Report Copy();
    }
}
