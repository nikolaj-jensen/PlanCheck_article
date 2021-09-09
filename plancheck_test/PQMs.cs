////////////////////////////////////////////////////////////////////////////////
// PQMs.cs
//
//  Plan Quality Metric calculators.  Requires DvhExtensions.GetDoseAtVolume
//  and DvhExtensions.GetVolumeAtDose that add these methods to PlanningItem.
//  
//  These calculators support the user defined plan quality metrics
//  in UserDefinedMetrics.cs.
//  
//  Each of these generate XML for each given metric, which is appended to the 
//  report that gets built by PQMReporter.
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
using System.Xml;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Xml.Linq;

namespace VMS.TPS
{
    public interface PlanQualityMetric
    {
        void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer);
    }

    //public interface PlanQualityMetricMissing
    //{
    //    void addPQMInfo(PlanningItem plan, string organ, XmlWriter writer);
    //}

    public interface PlanQualityReport
    {
        void addPQMInfo(PlanningItem plan, XmlWriter writer);
    }

    public static class PQMUtilities
    {
        public static XElement getDVXML(DoseValue dv)
        {
            return
              new XElement("DoseValue",
                new XAttribute("units", dv.UnitAsString),
                new XAttribute("calculated", true.ToString()),
                dv.ValueAsString);
        }
        public static XElement getDVXML2(double dmean_plan)
        {
            return
              new XElement("DoseValue",
                new XAttribute("units", "Gy"),
                new XAttribute("calculated", true.ToString()),
                dmean_plan.ToString());
        }
        public static XElement getEvaluateXML(DoseValue dv, DoseValue doseConstraint, double upperLimit)
        {
            double upperLimitMax = (doseConstraint.Dose * upperLimit);
            string pfw = "PASS";
            if (dv.Dose > doseConstraint.Dose && dv.Dose <= upperLimitMax)
                pfw = "WARN";
            else if (dv.Dose > upperLimitMax)
                pfw = "FAIL";

            return
              new XElement("Evaluate",
                new XElement("Limit",
                   new XAttribute("type", "upper"),
                   doseConstraint.ValueAsString),
                new XElement("Tolerance",
                   new XAttribute("type", "leq"),
                   upperLimit.ToString("0.000")),
                new XElement("Result",
                  new XElement("MaxLimit", upperLimitMax.ToString("0.000")),
                  new XElement("PFW", pfw)));
        }
        public static XElement getEvaluateXML2(double dmean_plan, double dmean_constraint, double upperLimit, string evaluation)
        {
            double upperLimitMax = (dmean_constraint * upperLimit);
            string pfw = evaluation;
            //if (dv.Dose > doseConstraint.Dose && dv.Dose <= upperLimitMax)
            //    pfw = "WARN";
            //else if (dv.Dose > upperLimitMax)
            //    pfw = "FAIL";

            return
              new XElement("Evaluate",
                new XElement("Limit",
                   new XAttribute("type", "upper"),
                   dmean_constraint.ToString()),
                new XElement("Tolerance",
                   new XAttribute("type", "leq"),
                   upperLimit.ToString("0.000")),
                new XElement("Result",
                  new XElement("MaxLimit", upperLimitMax.ToString("0.000")),
                  new XElement("PFW", pfw)));
        }
        public static XElement getEvaluateLowerLimitXML(DoseValue dv, DoseValue doseConstraint, double lowerLimit)
        {
            double lowerLimitMin = (doseConstraint.Dose * lowerLimit);
            string pfw = "PASS";
            if (dv.Dose < doseConstraint.Dose && dv.Dose >= lowerLimitMin)
                pfw = "WARN";
            else if (dv.Dose < lowerLimitMin)
                pfw = "FAIL";

            return
              new XElement("Evaluate",
                new XElement("Limit",
                   new XAttribute("type", "lower"),
                   doseConstraint.ValueAsString),
                new XElement("Tolerance",
                   new XAttribute("type", "geq"),
                   lowerLimit.ToString("0.000")),
                new XElement("Result",
                  new XElement("MinLimit", lowerLimitMin.ToString("0.000")),
                  new XElement("PFW", pfw)));
        }
        public enum DosePQMType { MaxDose = 0, MeanDose, MinDose };
        public static string ToString(this DosePQMType type) { if (type == DosePQMType.MaxDose) return "MaxDose"; if (type == DosePQMType.MeanDose) return "MeanDose"; return "unknown"; }
        public static void addDosePQMInfo2(string dosePQMName, string type, double dmean_plan, double dmean_constraint, double upperLimit, string color, int evaluation, XmlWriter writer)
        {
            ////ISW: Added check on whether this is a minimum dose (Dmin). In that case a lower limit has to be evaluated.
            //if (type == DosePQMType.MinDose)
            //{
            //    XElement pqm = new XElement("PQM",
            //    new XAttribute("type", type.ToString()),
            //    new XAttribute("name", dosePQMName),
            //    new XAttribute("color", color),
            //    getDVXML(dv));
            //    //getEvaluateLowerLimitXML(dv, doseConstraint, upperLimit));
            //    pqm.WriteTo(writer);
            //}
            //else
            //{
            string evaluationAsString_ = "";
            switch (evaluation)
            {
                case 1:
                    evaluationAsString_ = "PASS";
                    break;
                case 2:
                    evaluationAsString_ = "WARN";
                    break;
                case 3:
                    evaluationAsString_ = "FAIL";
                    break;
                case 4:
                    evaluationAsString_ = "Tjek dosis manuelt";
                    break;
                case 5:
                    evaluationAsString_ = "Organet er ikke indtegnet";
                    break;
                case 6:
                    evaluationAsString_ = "OBS";
                    break;
                default:
                    break;
            }

            XElement pqm = new XElement("PQM",
                  new XAttribute("type", type.ToString()),
                  new XAttribute("name", dosePQMName),
                  new XAttribute("color", color),
                  getDVXML2(dmean_plan),
                  getEvaluateXML2(dmean_plan, dmean_constraint, upperLimit, evaluationAsString_));
            pqm.WriteTo(writer);
            //}
        }
        public enum LimitType { upper = 0, lower };
        public static string ToString(this LimitType t)
        {
            switch (t)
            {
                case LimitType.upper:
                    return "upper";
                case LimitType.lower:
                    return "lower";
                default:
                    return "unknown";
            };
        }
    }
    struct DoseAtVolume : PlanQualityMetric
    {
        public double rel_vol_dose_constraint;
        public double rel_vol_dose_plan;
        public double upperLimit;
        public int evaluation;
        public double volume;
        public VolumePresentation vp;
        DoseValuePresentation dvp;
        string name;
        string color;
        PQMUtilities.LimitType lt;
        string type;

        public DoseAtVolume(string type_,string name_, double rel_vol_dose_constraint_, double rel_vol_dose_plan_, double volume_, double upperLimit_, string color_, int evaluation_,
                VolumePresentation vpp,
                DoseValuePresentation dvp_ = DoseValuePresentation.Absolute,
                PQMUtilities.LimitType lt_ = PQMUtilities.LimitType.upper)
        {
            name = name_;
            rel_vol_dose_constraint = rel_vol_dose_constraint_;
            rel_vol_dose_plan = rel_vol_dose_plan_;
            volume = volume_;
            upperLimit = upperLimit_;
            color = color_;
            evaluation = evaluation_;
            vp = vpp;
            dvp = dvp_;
            lt = lt_;
            type = type_;
        }
        public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
        {
            //ISW: Added check on whether the organ is conoured.
            DVHData dvh = plan.GetDVHCumulativeData(organ, dvp, VolumePresentation.Relative, 0.1);
            //if (dvh != null)
            //{
                writer.WriteStartElement("PQM");
                writer.WriteAttributeString("type", type);
                writer.WriteAttributeString("name", name);
                writer.WriteAttributeString("color", color);

                DoseValue dv = plan.GetDoseAtVolume(organ, volume, vp, dvp);
                writer.WriteStartElement("DoseValue");
                writer.WriteAttributeString("units", dv.UnitAsString);
                writer.WriteAttributeString("calculated", true.ToString());
                writer.WriteString(dv.ValueAsString);
                writer.WriteEndElement(); // </DoseValue>

                writer.WriteStartElement("Volume");
                writer.WriteAttributeString("units", vp.ToString());
                writer.WriteAttributeString("calculated", false.ToString());
                writer.WriteString(volume.ToString("0.000"));
                writer.WriteEndElement(); // </Volume>

                string limType = lt.ToString(), tolType;
                if (lt == PQMUtilities.LimitType.upper)
                    tolType = "leq";
                else
                    tolType = "geq";

                writer.WriteStartElement("Evaluate");
                writer.WriteStartElement("Limit");
                writer.WriteAttributeString("type", limType);
                writer.WriteString(rel_vol_dose_constraint.ToString());
                writer.WriteEndElement(); // </Limit>
                writer.WriteStartElement("Tolerance");
                writer.WriteAttributeString("type", tolType);
                writer.WriteString(upperLimit.ToString("0.000"));
                writer.WriteEndElement(); // </Tolerance>
                writer.WriteStartElement("Result");

                string evaluationAsString_ = "";
                switch (evaluation)
                {
                    case 1:
                        evaluationAsString_ = "PASS";
                        break;
                    case 2:
                        evaluationAsString_ = "WARN";
                        break;
                    case 3:
                        evaluationAsString_ = "FAIL";
                        break;
                    case 4:
                        evaluationAsString_ = "Tjek dosis manuelt";
                        break;
                    case 5:
                        evaluationAsString_ = "Organet er ikke indtegnet";
                        break;
                case 6:
                    evaluationAsString_ = "OBS";
                    break;
                default:
                        break;
                }
                writer.WriteElementString("PFW", evaluationAsString_);
                writer.WriteEndElement(); // </Result>
                writer.WriteEndElement(); // </Evaluate>

                writer.WriteEndElement(); // </PQM>
            //}
            //else
            //{
             //   XElement pqm = new XElement("PQM",
             //     new XElement("Error", string.Format("Structure '{0}' is not contoured.", organ.Id))
              //    );
              //  pqm.WriteTo(writer);
           // }
        }
    }
    struct GeneralInfo : PlanQualityReport
    {
        string name;
        string units;
        string planValue;
        string constraint;
        string color;
        int evaluation;
        public GeneralInfo(string name_, string units_, string planValue_, string constraint_, string color_, int evaluation_)
        {
            name = name_;
            units = units_;
            planValue = planValue_;
            constraint = constraint_;
            color = color_;
            evaluation = evaluation_;
        }
        public void addPQMInfo(PlanningItem plan, XmlWriter writer)
        {
           

            writer.WriteStartElement("PQM");
            //writer.WriteAttributeString("type", "Plan check");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("color", color);

            //DoseValue dv = plan.GetDoseAtVolume(organ, volume, vp, dvp);
            writer.WriteStartElement("DoseValue");
            writer.WriteAttributeString("units", units);
            writer.WriteAttributeString("calculated", true.ToString());
            writer.WriteString(planValue);
            writer.WriteEndElement(); // </DoseValue>
            writer.WriteStartElement("Evaluate");
            writer.WriteStartElement("Limit");
            writer.WriteString(constraint);
            writer.WriteEndElement(); // </Limit>
            writer.WriteStartElement("Result");
            string evaluationAsString_ = "";
            switch (evaluation)
            {
                case 1:
                    evaluationAsString_ = "PASS";
                    break;
                case 2:
                    evaluationAsString_ = "WARN";
                    break;
                case 3:
                    evaluationAsString_ = "FAIL";
                    break;
                case 6:
                    evaluationAsString_ = "OBS";
                    break;
                    //case 4:
                    //    evaluationAsString_ = "Tjek dosis manuelt";
                    //    break;
                    //case 5:
                    //    evaluationAsString_ = "Organet er ikke indtegnet";
                    //    break;
                    default:
                        break;
            }
            writer.WriteElementString("PFW", evaluationAsString_);
            writer.WriteEndElement(); // </Result>
            writer.WriteEndElement(); // </Evaluate>

            writer.WriteEndElement(); // </PQM>
        }
    }
    struct Manual : PlanQualityReport
    {
        string name;
        string text;
        string trafiklys;
        public Manual(string name_, string text_, string trafik_lys_)
        {
            name = name_;
            text = text_;
            trafiklys = trafik_lys_;
        }
        public void addPQMInfo(PlanningItem plan, XmlWriter writer)
        {
            writer.WriteStartElement("PQM");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("text", text);
            writer.WriteAttributeString("color", trafiklys);

            writer.WriteEndElement(); // </PQM>
        }
    }
    //struct StructureMissing : PlanQualityMetricMissing
    //{
    //    string name;
    //    string text;
    //    string trafiklys;
    //    public Manual(string name_, string text_, string trafik_lys_)
    //    {
    //        name = name_;
    //        text = text_;
    //        trafiklys = trafik_lys_;
    //    }
    //    public void addPQMInfo(PlanningItem plan, string oarMissing, XmlWriter writer)
    //    {
    //        writer.WriteStartElement("PQM");
    //        writer.WriteAttributeString("name", name);
    //        writer.WriteAttributeString("text", text);
    //        writer.WriteAttributeString("color", trafiklys);

    //        writer.WriteEndElement(); // </PQM>
    //    }
    //}

    struct MeanDoseLimit : PlanQualityMetric
    {
        //ISW: Added color to struct
        public double upperLimit;
        public string color;
        public int evaluation;
        DoseValuePresentation dvp;
        double dmean_plan;
        double dmean_constraint;
        //public DoseValue doseConstraint;
        private string name;
        string type;

        public MeanDoseLimit(string type_,string name_, double dmean_constraint_, double dmean_plan_, double upperLimit_, string color_, int evaluation_, DoseValuePresentation dvp_ = DoseValuePresentation.Absolute)
        {
            name = name_;
            dmean_plan = dmean_plan_;
            dmean_constraint = dmean_constraint_;
            upperLimit = upperLimit_;
            color = color_;
            evaluation = evaluation_;
            dvp = dvp_;
            type = type_;
        }
        public MeanDoseLimit(string type_,double dmean_plan_, double dmean_constraint_, double upperLimit_, string color_, int evaluation_,
                DoseValuePresentation dvp_ = DoseValuePresentation.Absolute)
        {
            name = "";
            dmean_plan = dmean_plan_;
            dmean_constraint = dmean_constraint_;
            upperLimit = upperLimit_;
            color = color_;
            evaluation = evaluation_;
            dvp = dvp_;
            type = type_;

        }
        public void addPQMInfo(PlanningItem plan, Structure organ, XmlWriter writer)
        {
            DVHData dvh = plan.GetDVHCumulativeData(organ, dvp, VolumePresentation.Relative, 0.1);
            if (dvh != null)
            {
                //DoseValue dv = dvh.MeanDose;
                //if (name.Length > 0)
                //{
                PQMUtilities.addDosePQMInfo2(name, type,dmean_plan, dmean_constraint, upperLimit, color, evaluation, writer);
                //}
                //else
                //{
                //    PQMUtilities.addDosePQMInfo2(PQMUtilities.DosePQMType.MeanDose, organ, plan, dv, doseConstraint, upperLimit, color, evaluation, writer);
                //}
            }
            else
            {
                XElement pqm = new XElement("PQM",
                  new XAttribute("type", type),
                  new XAttribute("name", "Dmean < " + dmean_constraint.ToString()),
                  new XElement("Error", string.Format("Structure '{0}' is not contoured.", organ.Id))
                  );
                pqm.WriteTo(writer);
            }
        }
    }
}