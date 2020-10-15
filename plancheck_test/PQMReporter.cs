////////////////////////////////////////////////////////////////////////////////
// PQMReporter.cs
//
//  A ESAPI PQMReporter that generates a report for the selected patient and
//  plan with various calculated Plan Quality Metrics for defined structures.
//  
// Applies to:  ESAPI v11, ESAPI v13.
//
// Copyright (c) 2014 Varian Medical Systems, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////
//
// The flag v13_ESAPI adds reporting on combined parotids,
// which is done by creating a new volume "Parotid Combined", 
// reporting on it, then removing it. V11 ESAPI doesn't support this.
//#define v13_ESAPI  // comment this line for version 11 ESAPI

using System;
using System.Text;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Security.Cryptography;

namespace VMS.TPS
{
    public abstract class PQMReporter
    {
        protected string eclipseVersion, scriptVersion;
        protected string currentuser;
        protected string rootPath;
        protected string stylesheet;
        protected ScriptContext context;
        protected XslCompiledTransform myXslTransform;
        protected string hospital;

        // override these methods
        abstract protected void dumpReportXML(ScriptContext context, Patient patient, StructureSet ss, PlanningItem plan, string xmlFilePath,string hospital);

        // implementation
        public PQMReporter(ScriptContext context_, string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, string planSetupStylesheet_ = @"gen_report.xsl")
        {
            rootPath = rootPath_;
            currentuser = userId;
            stylesheet = planSetupStylesheet_;
            eclipseVersion = eclipseVersion_;
            scriptVersion = scriptVersion_;
            context = context_;
            // load the stylesheet into memory, transform report XML into HTML, show that to the user.
            using (XmlReader stylesheetReader = XmlReader.Create(stylesheet))
            {
                stylesheetReader.ReadToDescendant("xsl:stylesheet");

                try
                {
                    myXslTransform = new XslCompiledTransform();
                    myXslTransform.Load(stylesheetReader);
                }
                catch (System.Xml.Xsl.XsltCompileException e)
                {
                    throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
                }
                catch (System.Xml.Xsl.XsltException e)
                {
                    throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
                }
            }
        }

        public PQMReporter(ScriptContext context_, string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, XmlReader stylesheetReader, string hospital)
        {
            rootPath = rootPath_;
            currentuser = userId;
            stylesheet = "internal";
            eclipseVersion = eclipseVersion_;
            scriptVersion = scriptVersion_;
            context = context_;

            // load the PQM stylesheet into memory, transform report XML into HTML, show that to the user.
            stylesheetReader.ReadToDescendant("xsl:stylesheet");

            try
            {
                myXslTransform = new XslCompiledTransform();
                myXslTransform.Load(stylesheetReader);
            }
            catch (System.Xml.Xsl.XsltCompileException e)
            {
                throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
            }
            catch (System.Xml.Xsl.XsltException e)
            {
                throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
            }
        }
        string MakeFilenameValid(string s)
        {
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char ch in invalidChars)
            {
                s = s.Replace(ch, '_');
            }
            return s;
        }
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// This method creates a Plan Quality Metric report for the specified plan.
        /// </summary>
        /// <param name="patient">loaded patient</param>
        /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
        /// <param name="plan">Plan for which the report is going to be generated.</param>
        /// <param name="rootPath">root directory for the report.  This method creates a subdirectory
        ///  'patientid' under the root, then creates the xml and html reports in the subdirectory.</param>
        /// <param name="userId">User whose id will be stamped on the report.</param>
        /// <param name="xmlFilePath">Raw unformatted XML report.</param>
        //---------------------------------------------------------------------------------------------
        public string generateReport(ScriptContext context, Patient patient, StructureSet ss, PlanningItem plan, string hospital, out string xmlFilePath)
        {

            rootPath = @"\\rgharia005\planscript\fysiker";
            string anon_ID;
            //Generate MD5 hash for patient ID, this will be used as anonymized ID For patient in databased.
            string GetMD5Hash(MD5 md5Hash, string input)
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            using (MD5 md5Hash = MD5.Create())
            {
                anon_ID = GetMD5Hash(md5Hash, patient.LastName + patient.FirstName + patient.Id);
            }

            
            string rootDir = string.Format(@"{0}\{1}", rootPath, MakeFilenameValid(anon_ID)); //XXXXXXXXXXXXXXXXXXXXXXX DB id og sæt nummer på fil navn
            Directory.CreateDirectory(rootDir);
            string fileRoot = string.Format(@"{0}\PQMReport-{1}", rootDir, MakeFilenameValid(plan.Id) + "  " + MakeFilenameValid(DateTime.Now.ToString()));
            // build exported XML filename, put it in the root path (likely is users temp directory)
            xmlFilePath = string.Format(@"{0}.xml", fileRoot);
            string htmlFilePath = string.Format(@"{0}.html", fileRoot);

            dumpReportXML(context, patient, ss, plan, xmlFilePath,hospital);
            
            // PQM stylesheet should already be loaded into memory, transform report XML into HTML, show that to the user.
            try
            {
                myXslTransform.Transform(xmlFilePath, htmlFilePath);
                try
                {

                    DB_parser_copy output_db = new DB_parser_copy();

                    bool out_status = output_db.Output_check( context.Course.Id, plan.Id, context.CurrentUser.Id, htmlFilePath, anon_ID, hospital);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show(e.ToString());
                }
                return htmlFilePath;
           
            }
            catch (System.Xml.Xsl.XsltCompileException e)
            {
                throw new ApplicationException("Failed to load xsl stylesheet '" + stylesheet + "'. details:\n" + e.ToString());
            }
            catch (System.Xml.Xsl.XsltException e)
            {
                throw new ApplicationException("General stylesheet problem when applying '" + stylesheet + "'. details:\n" + e.ToString());
            }

        }

        //     combined parotids (cut this out for version 11, won't work)
#if v13_ESAPI
      if (patient.CanModifyData())
      {
        int count = 0;
        patient.BeginModifications();   // enable writing with this script.
        // create the empty "Parotid Combined" structure
        string ID = "Parotid Combined";
        Structure parotid_combined = ss.AddStructure("AVOIDANCE", ID);

        // search for left parotid, if found combine it into 'parotid_combined'
        foreach (string volumeId in ParotidLeft.searchIds)
        {
          Structure oar = (from s in ss.Structures
                           where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                           select s).FirstOrDefault();
          if (oar != null)
          {
            count++;
            parotid_combined.SegmentVolume = parotid_combined.Or(oar);
            break;
          }
        }

        // search for right parotid, if found combine it into 'parotid_combined'
        foreach (string volumeId in ParotidRight.searchIds)
        {
          Structure oar = (from s in ss.Structures
                           where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                           select s).FirstOrDefault();
          if (oar != null)
          {
            count++;
            parotid_combined.SegmentVolume = parotid_combined.Or(oar);
            break;
          }
        }
        if (count > 0)
        {
          string[] searchIds = { ID };
          addStructurePQM(plan, ss, searchIds, ParotidsCombined.PQMs, writer);
        }
        ss.RemoveStructure(parotid_combined);
      }
#endif
        public static class getGeneralInfo
        {
            public static PlanQualityReport[] getPQMs(string name_, string units, string constraint_, string planValue_, string color_, int evaluation_)
            {
                PlanQualityReport[] PQMs =
                      {
                 new GeneralInfo(name_, units, planValue_, constraint_,color_, evaluation_),
            };
                return PQMs;
            }
        };
        public static class getManual
        {
            public static PlanQualityReport[] getPQMs(string name_,string text_, string trafik_lys_)
            {
                PlanQualityReport[] PQMs =
                      {
                 new Manual(name_, text_,trafik_lys_),
            };
                return PQMs;
            }
        };
        //public static class getManualDose
        //{
        //    public static PlanQualityReport[] getPQMs(string name_, string text_, string trafik_lys_)
        //    {
        //        PlanQualityReport[] PQMs =
        //              {
        //         new Manual(name_, text_,trafik_lys_),
        //    };
        //        return PQMs;
        //    }
        //};
        public static class getMeanDosePQMs
        {
            public static PlanQualityMetric[] getPQMs(string type_,string name_, double dmean_constraint_, double dmean_plan_, double upperLimit_, string trafik_lys_, int evaluation_)
            {
                PlanQualityMetric[] PQMs =
                      {
                 new MeanDoseLimit(type_, name_, dmean_constraint_, dmean_plan_, 1.0, trafik_lys_, evaluation_),
            };
                return PQMs;
            }
        };
        public static class getRelativeDoseAtVolume
        {
            public static PlanQualityMetric[] getPQMs(string type_,string name_, double rel_vol_dose_constraint_, double rel_vol_dose_plan_, double volume_constraint_, double upperLimit_, string trafik_lys_, int evaluation_)
            {
                PlanQualityMetric[] PQMs =
                      {
                 new DoseAtVolume(type_,name_, rel_vol_dose_constraint_, rel_vol_dose_plan_, volume_constraint_, upperLimit_, trafik_lys_, evaluation_,VolumePresentation.Relative),
            };
                return PQMs;
            }
        };
        public static class getAbsoluteDoseAtVolume
        {
            public static PlanQualityMetric[] getPQMs(string type_,string name_, double rel_vol_dose_constraint_, double rel_vol_dose_plan_, double volume_constraint_, double upperLimit_, string trafik_lys_, int evaluation_)
            {
                PlanQualityMetric[] PQMs =
                      {
                 new DoseAtVolume(type_,name_, rel_vol_dose_constraint_, rel_vol_dose_plan_, volume_constraint_, upperLimit_, trafik_lys_, evaluation_, VolumePresentation.AbsoluteCm3),
            };
                return PQMs;
            }
        };
        /// <summary>
        /// ISW: This function gets the XML statistics for the html report
        /// </summary>
        /// <param name="patient">patient</param>
        /// <param name="ss">structure set</param>
        /// <param name="plan">current plan</param>
        /// <param name="writer">the writer for the html report</param>
        /// <param name="name_">name of the organ evaluated</param>
        /// <param name="dmean_constraint_">constraint</param>
        /// <param name="dmean_plan_">the actual mean dose to the organ in the plan</param>
        /// <param name="upperLimit_">a parameter not used at the moment</param>
        /// <param name="trafik_lys_">decides the importance of the constraint</param>
        /// <param name="evaluation_">1 = PASS, 2 = WARN, 3 = FAIL, 4 = tjekkes manuelt, 5 = organet er ikke indtegnet</param>
        public static void WriteDoseStatisticsXML_MeanDose(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string type_,string name_, double dmean_constraint_, double dmean_plan_, double upperLimit_, string trafik_lys_, int evaluation_)
        {
            PlanQualityMetric[] PQMs = getMeanDosePQMs.getPQMs(type_,name_, dmean_constraint_, dmean_plan_, 1.0, trafik_lys_, evaluation_);
            string[] searchIds = { name_ };
            addStructurePQM(plan, ss, searchIds, PQMs, writer);
        }
        public static void WriteDoseStatisticsXML_RelativeVolume(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer,string type_, string name_, double rel_vol_dose_constraint_, double rel_vol_dose_plan_, double volume_constraint_, double upperLimit_, string trafik_lys_, int evaluation_)
        {
            PlanQualityMetric[] PQMs = getRelativeDoseAtVolume.getPQMs(type_,name_, rel_vol_dose_constraint_, rel_vol_dose_plan_, volume_constraint_, upperLimit_, trafik_lys_, evaluation_);
            string[] searchIds = { name_ };
            addStructurePQM(plan, ss, searchIds, PQMs, writer);
        }
        public static void WriteDoseStatisticsXML_AbsoluteVolume(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer,string type_, string name_, double abs_vol_dose_constraint_, double abs_vol_dose_plan_, double volume_constraint_, double upperLimit_, string trafik_lys_, int evaluation_)
        {
            PlanQualityMetric[] PQMs = getAbsoluteDoseAtVolume.getPQMs(type_,name_, abs_vol_dose_constraint_, abs_vol_dose_plan_, volume_constraint_, upperLimit_, trafik_lys_, evaluation_);
            string[] searchIds = { name_ };
            addStructurePQM(plan, ss, searchIds, PQMs, writer);
        }
        public static void WriteManualDoseStatisticsXML(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string name_, string text_, string trafik_lys_)
        {
            PlanQualityReport[] PQMs = getManual.getPQMs(name_, text_, trafik_lys_);
            string[] searchIds = { name_ };
            addGeneralPQM(plan, name_, PQMs, writer);
        }
        public static void WriteGeneralStatisticsXML(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string name_, string units, string constraint_, string planValue_, string trafik_lys_, int evaluation_)
        {
            PlanQualityReport[] PQMs = getGeneralInfo.getPQMs(name_, units, constraint_, planValue_, trafik_lys_, evaluation_);
            string[] searchIds = { name_ };
            addGeneralPQM(plan, name_, PQMs, writer);
        }
        public static void WriteManualStatisticsXML(Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string name_, string text_, string trafik_lys_)
        {
            PlanQualityReport[] PQMs = getManual.getPQMs(name_,text_, trafik_lys_);
            string[] searchIds = { name_ };
            addGeneralPQM(plan, name_, PQMs, writer);
        }
        private static void addStructurePQM(PlanningItem plan, StructureSet ss, string[] structureIds, PlanQualityMetric[] pqmStats, XmlWriter writer)
        {
            // search through the list of structure ids until we find one
            Structure oar = null;
            string actualStructId = null;
            foreach (string volumeId in structureIds)
            {
                oar = (from s in ss.Structures
                       where s.Id.ToUpper().CompareTo(volumeId.ToUpper()) == 0
                       select s).FirstOrDefault();
                if (oar != null)
                {
                    actualStructId = oar.Id;
                    break;
                }
            }

            writer.WriteStartElement("Structure");
            writer.WriteAttributeString("Id", actualStructId);

            bool bStructureFound = (oar != null);
            writer.WriteAttributeString("present", bStructureFound.ToString());
            if (bStructureFound)
            {
                
                writer.WriteElementString("Volume", oar.Volume.ToString("0.00000"));
                writer.WriteStartElement("PQMs"); // PQM = Plan Quality Metric

                foreach (PlanQualityMetric pqm in pqmStats)
                {
                    pqm.addPQMInfo(plan, oar, writer);
                }

                writer.WriteEndElement(); // </PQMs>
            }

            writer.WriteEndElement(); // </Structure>
        }
        private static void addGeneralPQM(PlanningItem plan, string name, PlanQualityReport[] pqmStats, XmlWriter writer)
        {

            writer.WriteStartElement("Structure");
            writer.WriteAttributeString("Id", name);

            bool pqmPresent = true;
            writer.WriteAttributeString("present", pqmPresent.ToString());
            writer.WriteStartElement("PQMs"); // PQM = Plan Quality Metric
            foreach (PlanQualityReport pqm in pqmStats)
            {
                pqm.addPQMInfo(plan, writer);
            }
            writer.WriteEndElement(); // </PQMs>
            //}
            writer.WriteEndElement(); // </Structure>
        }
        private static void addManualPQM(PlanningItem plan, string name, PlanQualityReport[] pqmStats, XmlWriter writer)
        {

            writer.WriteStartElement("Structure");
            writer.WriteAttributeString("Id", name);

            bool pqmPresent = true;
            writer.WriteAttributeString("present", pqmPresent.ToString());

            writer.WriteStartElement("PQMs"); // PQM = Plan Quality Metric
            foreach (PlanQualityReport pqm in pqmStats)
            {
                pqm.addPQMInfo(plan, writer);
            }
            writer.WriteEndElement(); // </PQMs>
            //}
            writer.WriteEndElement(); // </Structure>
        }
        public void WritePatientXML(Patient patient, XmlWriter writer)
        {
            string anon_ID;
            //Generate MD5 hash for patient ID, this will be used as anonymized ID For patient in databased.
            using (MD5 md5Hash = MD5.Create())
            {
                anon_ID = GetMD5Hash(md5Hash,patient.LastName+patient.FirstName+patient.Id);
            }
            string GetMD5Hash(MD5 md5Hash, string input)
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            writer.WriteAttributeString("Id", anon_ID);
            writer.WriteElementString("CreationDateTime", patient.CreationDateTime.ToString());
           // writer.WriteElementString("Id2", patient.Id2);
            //writer.WriteElementString("SSN", patient.SSN);
            //writer.WriteElementString("FirstName", patient.FirstName);
            //writer.WriteElementString("MiddleName", patient.MiddleName);
            //writer.WriteElementString("LastName", patient.LastName);
            //writer.WriteElementString("Sex", patient.Sex);
            //writer.WriteElementString("PrimaryOncologistId", patient.PrimaryOncologistId);
        }
        public enum CtrlPtSelector { NoControlPoints = 0, IncludeControlPoints };
        public enum MLCSelector { NoMLC = 0, IncludeMLC };
        public void WritePlanXML(PlanSetup plan, XmlWriter writer, CtrlPtSelector writeCtrlPts = CtrlPtSelector.NoControlPoints, MLCSelector mlc = MLCSelector.NoMLC)
        {
            Course course = plan.Course;
            writer.WriteStartElement("Courses");
            writer.WriteStartElement("Course");
            writer.WriteAttributeString("Id", course.Id);
            writer.WriteAttributeString("Name", course.Name);
            writer.WriteAttributeString("Comment", course.Comment);
            writer.WriteElementString("StartDateTime", course.StartDateTime.ToString());
            writer.WriteStartElement("PlanSetups");
            writer.WriteStartElement("PlanSetup");
            plan.WriteXml(writer);
            if (writeCtrlPts == CtrlPtSelector.IncludeControlPoints)
            {
                WriteBeamAndControlPoints(plan, writer, mlc);
            }
            writer.WriteEndElement(); // </PlanSetup>
            writer.WriteEndElement(); // </PlanSetups>
            writer.WriteEndElement(); // </Course>
            writer.WriteEndElement(); // </Courses>
        }
        protected void WriteBeamAndControlPoints(PlanSetup plan, XmlWriter writer, MLCSelector mlc)
        {
            writer.WriteStartElement("BeamAndControlPoints");
            foreach (var beam in plan.Beams)
            {
                writer.WriteStartElement("Beam");
                beam.WriteXml(writer);
                // ... and controlpoints
                writer.WriteStartElement("ControlPoints");
                beam.ControlPoints.WriteXml(writer);
                foreach (var controlPoint in beam.ControlPoints)
                {
                    writer.WriteStartElement("ControlPoint");
                    controlPoint.WriteXml(writer);
                    if (mlc == MLCSelector.IncludeMLC)
                    {
                        float[,] lp = controlPoint.LeafPositions;
                        if (lp.Length > 1)
                        {
                            writer.WriteStartElement("LeafPositions");
                            for (int i = 0; i <= lp.GetUpperBound(0); i++)
                            {
                                writer.WriteStartElement("Bank");
                                writer.WriteAttributeString("number", i.ToString());
                                for (int j = 0; j <= lp.GetUpperBound(1); j++)
                                {
                                    float f = lp[i, j];
                                    writer.WriteString(f.ToString() + " ");
                                }
                                writer.WriteEndElement();//</Bank>
                            }
                            writer.WriteEndElement();//</LeafPositions>
                        }
                    }
                    writer.WriteEndElement();//</ControlPoint>
                }
                writer.WriteEndElement();//</ControlPoints>
                writer.WriteEndElement(); // </Beam>
            }
            writer.WriteEndElement(); // </BeamAndControlPoints>
        }
    }

    public class PlanSetupReporter : PQMReporter
    {
        public PlanSetupReporter(ScriptContext context_, string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, string planSetupStylesheet_ = @"gen_report.xsl")
          : base(context_,rootPath_, userId, eclipseVersion_, scriptVersion_, planSetupStylesheet_)
        { }
        public PlanSetupReporter(ScriptContext context_, string rootPath_, string userId, string eclipseVersion_, string scriptVersion_, XmlReader stylesheetReader,string hospital)
          : base(context_,rootPath_, userId, eclipseVersion_, scriptVersion_, stylesheetReader, hospital)
        {
        }
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// This method creates a Plan Quality Metric report for the specified plan.
        /// </summary>
        /// <param name="patient">loaded patient</param>
        /// <param name="ss">structure set to use while generating Plan Quality Metrics</param>
        /// <param name="plan">Plan for which the report is going to be generated.</param>
        /// <param name="rootPath">root directory for the report.  This method creates a subdirectory
        ///  'patientid' under the root, then creates the xml and html reports in the subdirectory.</param>
        /// <param name="userId">User whose id will be stamped on the report.</param>
        //---------------------------------------------------------------------------------------------
        override protected void dumpReportXML(ScriptContext context_, Patient patient, StructureSet ss, PlanningItem plan_, string sXMLPath,string hospital)
        {
            if (!(plan_ is PlanSetup))
                throw new ApplicationException("PlanSetupReporter should be used only for PlanSetup types!");

            PlanSetup plan = (PlanSetup)plan_;
            ScriptContext context = context_;


            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            System.IO.MemoryStream mStream = new System.IO.MemoryStream();
            XmlWriter writer = XmlWriter.Create(mStream, settings);
            writer.WriteStartDocument(true);
            writer.WriteStartElement("PlanQualityReport");
            writer.WriteAttributeString("created", DateTime.Now.ToString());
            writer.WriteAttributeString("userid", currentuser);
            writer.WriteAttributeString("eclipseVersion", eclipseVersion);
            writer.WriteAttributeString("scriptVersion", scriptVersion);
            //Writing to report/ISW
            //string _totalDoseCheckText = "ISAK ER GENIAL";
            //writer.WriteAttributeString("totalDoseCheckText", _totalDoseCheckText.ToUpper());

            writer.WriteStartElement("Patient");
            WritePatientXML(patient, writer);
            WritePlanXML(plan, writer, CtrlPtSelector.IncludeControlPoints);
            writer.WriteEndElement(); // </Patient>

            writer.WriteStartElement("DoseStatistics");
            //120917: Add call to dose function in PlanCheck Test. Input for the function must be (Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
            Script.getDoseLimits(context, patient, ss, plan, writer, hospital);
            writer.WriteEndElement(); // </DoseStatistics>
            writer.WriteStartElement("PlanStatistics");
            Script.getEverythingButDosis(context, patient, ss, plan, writer, hospital);
            writer.WriteEndElement(); // </PlanStatistics>
            writer.WriteStartElement("Manual");
            Script.manual(context, patient, ss, plan, writer, hospital);
            //Script.Manual_dose(context, patient, ss, plan, writer);
            writer.WriteEndElement(); // </Manual>

            writer.WriteEndElement(); // </PlanQualityReport>
            writer.WriteEndDocument();
            writer.Flush();
            mStream.Flush();

            // write the XML file report.
            using (System.IO.FileStream file = new System.IO.FileStream(sXMLPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                // Have to rewind the MemoryStream in order to read its contents. 
                mStream.Position = 0;
                mStream.CopyTo(file);
                file.Flush();
                file.Close();
            }

            writer.Close();
            mStream.Close();
            Script.Cleanup();
        }

    }
}
