using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using VMS.TPS;
using System.Security.Cryptography;

namespace VMS.TPS
{
    public class Constraint
    {
        public string[] type;
        public string[] navn;
        public bool[] req;
        public double[] dmean;
        public double[] rel_vol_dose;
        public double[] rel_vol;
        public double[] abs_vol_dose;
        public double[] abs_vol;
        public int[] mere_end_mindre_end;
        public string[] trafik_lys;
        public string[] kommentar;
    }

    

    public static class DVH_check
    {
        public static string DVH_out_string = null;
    }



    public class Script
    {
        

        public static void dmean_check(string navn, bool req, double dmean, int mere_end_mindre_end, string trafik_lys, ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
        {
            int evaluation;
            
            if (!context.StructureSet.Structures.Any(n => n.Id.ToLower() == navn.ToLower()))
            {
                if (req)
                {

                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, navn, "", "", string.Format("{0}{1}", navn, " missing"), trafik_lys, 3);
                }
            }
            else if (context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).First().Volume > .001)
            {
                double dmean_plan = Math.Round(context.PlanSetup.GetDVHCumulativeData(context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).FirstOrDefault(), DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1).MeanDose.Dose, 1);

                
                switch (mere_end_mindre_end)
                {
                    case 1:
                        if (dmean_plan >= dmean-0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> > " + dmean +"Gy" , navn , dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }
                        else
                        {
                            
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> > " + dmean + "Gy", navn, dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }

                        break;
                    case 3:
                        if (dmean_plan <= dmean+0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> < " + dmean + "Gy", navn , dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }
                        else
                        {
                            
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> < " + dmean + "Gy", navn, dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }

                        break;
                    case 2:
                        if (Math.Abs((dmean_plan / dmean) - 1) < 0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> = " + dmean + "Gy", navn, dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }
                        else
                        {
                            
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_MeanDose(patient, ss, plan, writer, "D<sub>mean</sub> = " + dmean + "Gy",navn, dmean, dmean_plan, 1.0, trafik_lys, evaluation);
                        }
                        break;
                    case 9:
                       
                        
                        break;
                }
                
            }


            //return pass;
        }


        public static void rel_vol_check(string navn, bool req, double rel_vol_dose, double rel_vol, int mere_end_mindre_end, string trafik_lys, ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
        {

            int evaluation;
            if (!context.StructureSet.Structures.Any(n => n.Id.ToLower() == navn.ToLower()))
            {
                if (req)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, navn, "", "", string.Format("{0}{1}", navn, " missing"), trafik_lys, 3);
                }
            }
            else if ((context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).FirstOrDefault().Volume > 0.001))
            {


                double dose_plan = Math.Round(context.PlanSetup.GetDoseAtVolume(context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).FirstOrDefault(), rel_vol, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose, 1);

                switch (mere_end_mindre_end)
                {
                    case 1:
                        if (dose_plan >= rel_vol_dose-0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> > " + rel_vol_dose + "Gy" , navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                        }
                        else
                        {
                            
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> > " + rel_vol_dose + "Gy", navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                        }

                        break;
                    case 3:
                        if (dose_plan <= rel_vol_dose+0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> < " + rel_vol_dose + "Gy", navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                            
                        }
                        else
                        {
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> < " + rel_vol_dose + "Gy", navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                            
                        }
                        break;
                    case 2:
                        if (Math.Abs((dose_plan / rel_vol_dose) - 1) < 0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                            {
                                evaluation = 1;
                                VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> = " + rel_vol_dose + "Gy", navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                                
                            }
                            else
                            {
                                evaluation = 3;
                                VMS.TPS.PQMReporter.WriteDoseStatisticsXML_RelativeVolume(patient, ss, plan, writer, "D<sub>" + rel_vol + "%</sub> = " + rel_vol_dose + "Gy", navn, rel_vol_dose, dose_plan, rel_vol, 1.0, trafik_lys, evaluation);
                                
                            }
                        break;
                    case 9:
                        break;

                }
               
            }


            //return pass;
        }

        public static void abs_vol_check(string navn, bool req, double abs_vol_dose, double abs_vol, int mere_end_mindre_end, string trafik_lys, ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
        {
            
            int evaluation;
            if (!context.StructureSet.Structures.Any(n => n.Id.ToLower() == navn.ToLower()))
            {
                if (req)
                {
                    
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, navn, "", "", string.Format("{0}{1}", navn, " missing"), trafik_lys, 3);
                }
            }
            else if (context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).First().Volume > .001)
            {

                double dose_plan = Math.Round(context.PlanSetup.GetDoseAtVolume(context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).First(), abs_vol, VolumePresentation.AbsoluteCm3, DoseValuePresentation.Absolute).Dose, 1);
                switch (mere_end_mindre_end)
                {
                    case 1:
                        if (dose_plan >= abs_vol_dose-0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> > " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                            
                        }
                        else
                        {
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> > " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                           
                        }

                        break;
                    case 3:
                        if (dose_plan <= abs_vol_dose+0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                        {
                            evaluation = 1;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> < " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                            
                        }
                        else
                        {
                            evaluation = 3;
                            VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> < " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                            
                        }
                        break;
                    case 2:
                        if (Math.Abs((dose_plan / abs_vol_dose) - 1) < 0.01) //hvis mere_end er 1 skal dmean_plan være større end dmean og omvendt hvis mere_end er 3, derfor byttes derfortegn hvis mere_end er 3
                            {
                                evaluation = 1;
                                VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> = " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                                
                            }
                            else
                            {
                                evaluation = 3;
                                VMS.TPS.PQMReporter.WriteDoseStatisticsXML_AbsoluteVolume(patient, ss, plan, writer, "D<sub>" + abs_vol + "cm3</sub> = " + abs_vol_dose + "Gy", navn, abs_vol_dose, dose_plan, abs_vol, 1.0, trafik_lys, evaluation);
                                
                            }
                        break;
                    case 9:
                        
                        break;

                }
                
            }
            
        }


        public static void rapport(string navn, ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer,string trafik_lys)
        {
            
            if (context.StructureSet.Structures.Any(n => n.Id.ToLower() == navn.ToLower()) && context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).First().Volume > 0.1)
            {
                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, navn, "", "", string.Format("{0}{1}", navn, " exists"), trafik_lys, 1);
            }
            else
            {
                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, navn, "", "", string.Format("{0}{1}", navn, " missing"), trafik_lys, 3);
            }
                       
        }

        //public bool manual(string navn, bool req, string kommentar, ScriptContext context)
        //{
        //    bool pass = false;

        //    if (req)

        //        if (context.StructureSet.Structures.Any(n => n.Id.ToLower() == navn.ToLower()) && context.StructureSet.Structures.Where(n => n.Id.ToLower() == navn.ToLower()).First().HasSegment)
        //        {
        //            pass = true;
        //        }
        //        else
        //        {
        //            DVH_check.DVH_out_string = DVH_check.DVH_out_string + Environment.NewLine + "Mangler indtegning af " + navn + " som skal checkes manuelt for " + kommentar;

        //            pass = false;
        //            return pass;
        //        }
        //    string out_string = "Check manuelt at: " + navn + " " + kommentar;
        //    DVH_check.DVH_out_string = DVH_check.DVH_out_string + Environment.NewLine + out_string;
        //    return pass;
        //}



        public Script()
        {
        }

        public void Execute(ScriptContext context /*, System.Windows.Window window*/)
        {

            try
            {


                string temp = System.Environment.GetEnvironmentVariable("TEMP");
                string eclipseVersion = System.Reflection.Assembly.GetAssembly
                  (typeof(VMS.TPS.Common.Model.API.Application)).GetName().Version.ToString();
                string scriptVersion = "1.0.10";

                if (context.Patient == null || context.PlanSetup == null)
                {
                    System.Windows.MessageBox.Show("Please select the external beam plan that you would like evaluate.",
                                    "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (context.PlanSetup.StructureSet == null)
                {
                    System.Windows.MessageBox.Show("Loaded plan does not have a structure set.",
                                   "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (context.PlanSetup.Dose == null)
                {
                    System.Windows.MessageBox.Show("Loaded plan has no dose: Please select an external beam plan with dose calculated that you would like evaluate.",
                                    "Varian Developer", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string hospital = null;
                switch (context.Patient.Hospital.Id.ToLower())
                {
                    case "rigshospitalet":
                        hospital = "RH";
                        break;
                    case "Herlev":
                        hospital = "Herlev";
                        break;

                }
                string user = null;
                user = context.CurrentUser.Id.ToLower();
                // gen_report.xsl is automatically compiled in when the build action for the VS project is set to 
                // "Embedded Resource".  See online help for Build Action.  Read the embedded stylesheet file from the DLL.
                Stream stylesheet = Assembly.GetExecutingAssembly().GetManifestResourceStream("plancheck_test.gen_report.xsl");
                XmlReader stylesheetReader = XmlReader.Create(new StreamReader(stylesheet));

                VMS.TPS.PlanSetupReporter reporter = new PlanSetupReporter(context, temp, context.CurrentUser.Id, eclipseVersion, scriptVersion, stylesheetReader, hospital);
                Patient patient = context.Patient;
                PlanSetup plan = context.PlanSetup;
                StructureSet ss = context.StructureSet;
                
                
                string xmlReportPath;
                string htmlReportPath = reporter.generateReport(context, patient, plan.StructureSet, plan, hospital, user, out xmlReportPath);

                // 'Start' generated HTML file to launch browser window

                System.Diagnostics.Process.Start(htmlReportPath);

                // Sleep for a few seconds to let internet browser window to start

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            catch (Exception e)
            {

                System.Windows.MessageBox.Show(e.ToString());
            }

        }

        
        public static void getDoseLimits(ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string hospital)
        {
            try
            {


                string plan_name;
                if (context.PlanSetup.Id.ToLower().Substring(0, 2) == "cm")
                {
                    if (context.PlanSetup.Beams.First().IsocenterPosition.x < 0)
                    {
                        plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "h";
                    }
                    else
                    {
                        plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "v";
                    }

                }
                else if (context.PlanSetup.Id.Substring(0, 2).ToLower() == "hp" && context.PlanSetup.Id.ToLower().Contains("dib"))
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 8);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srk") //Der er 6 tegn i SRK koder, derfor skal de håndteres lidt anderledes
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 6);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srt")
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 3);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "gyn")
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 6);
                }
                else if (context.PlanSetup.Id.Length > 4)// resten af koderne har kun 5
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5);
                }

                else
                {
                    plan_name = context.PlanSetup.Id.ToLower() + "nnnnnnn";
                }

                if (plan_name.Substring(0, 2) != "cm" && plan_name.Substring(plan_name.Length - 3) == "000")// HVIS 000 kode så laver den en XX000_d hvor d er fraktionsdosis så der kan oprettes i diagnose tabel og prioritet et sæt for den situation
                {
                    plan_name = plan_name + "_" + context.PlanSetup.DosePerFraction.Dose.ToString();
                }
                else if (plan_name.Substring(0, 2) == "cm" && plan_name.Substring(plan_name.Length - 4, 3) == "000")
                {
                    plan_name = plan_name + "_" + context.PlanSetup.DosePerFraction.Dose.ToString();
                }

                string out_string = "";

                //Check at dosis er beregnet


                if (!context.PlanSetup.IsDoseValid)
                {
                    out_string += System.Environment.NewLine + "Dosis er ikke beregnet, check stopper her.";
                    System.Windows.MessageBox.Show(out_string);
                    return;
                }

                DB_parser_copy get_diagnose_from_DB = new DB_parser_copy();

                bool DB_status = get_diagnose_from_DB.Read_diagnose(plan_name, hospital);

                int fx = 0;
                int diag_index = 0;
                double fx_dosis = 0;
                double norm_procent = 100;
                string norm_target = "ptv";
                string norm_volume = "dmean";
                string setup = null;
                if (!DB_status)
                {
                    
                }
                else
                {
                    diag_index = DB_parser_copy.DB_diag.DB_diag_code;
                    fx = DB_parser_copy.DB_diag.DB_fraktioner;
                    fx_dosis = DB_parser_copy.DB_diag.DB_fx_dosis;
                    norm_procent = DB_parser_copy.DB_diag.DB_norm_procent; 
                    if (DB_parser_copy.DB_diag.DB_norm_target != "0") { norm_target = DB_parser_copy.DB_diag.DB_norm_target; }
                    if (DB_parser_copy.DB_diag.DB_norm_volume!= "0") { norm_volume = DB_parser_copy.DB_diag.DB_norm_volume; }
                    setup = DB_parser_copy.DB_diag.DB_setup;
                }

                bool DB_structures_status = get_diagnose_from_DB.Read_priority(diag_index,hospital);

                if (DB_parser_copy.DB_structures.DB_strukt==null || DB_parser_copy.DB_structures.DB_strukt.Length < 1)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No constraints found in database", "", "", "", "g", 3);
                }

                int[] prioritet = new int[0];
                if (DB_structures_status)
                {

                    prioritet = DB_parser_copy.DB_structures.DB_strukt;

                    int counter = 0;
                    Constraint constraints = new Constraint();

                    if (prioritet != null)
                    {
                        constraints.type = new string[prioritet.Length];
                        constraints.navn = new string[prioritet.Length];
                        constraints.req = new bool[prioritet.Length];
                        constraints.dmean = new double[prioritet.Length];
                        constraints.rel_vol_dose = new double[prioritet.Length];
                        constraints.rel_vol = new double[prioritet.Length];
                        constraints.abs_vol_dose = new double[prioritet.Length];
                        constraints.abs_vol = new double[prioritet.Length];
                        constraints.mere_end_mindre_end = new int[prioritet.Length];
                        constraints.trafik_lys = new string[prioritet.Length];
                        constraints.kommentar = new string[prioritet.Length];

                        while (counter < prioritet.Length)
                        {
                            
                            if (get_diagnose_from_DB.Read_structure(prioritet[counter],hospital))
                            {


                                constraints.type[counter] = DB_parser_copy.DB_constraints.type;
                                constraints.navn[counter] = DB_parser_copy.DB_constraints.DB_navn;
                                constraints.req[counter] = DB_parser_copy.DB_constraints.DB_req;
                                constraints.dmean[counter] = DB_parser_copy.DB_constraints.DB_dmean;
                                constraints.rel_vol_dose[counter] = DB_parser_copy.DB_constraints.DB_rel_vol_dose;
                                constraints.rel_vol[counter] = DB_parser_copy.DB_constraints.DB_rel_vol;
                                constraints.abs_vol_dose[counter] = DB_parser_copy.DB_constraints.DB_abs_vol_dose;
                                constraints.abs_vol[counter] = DB_parser_copy.DB_constraints.DB_abs_vol;
                                constraints.mere_end_mindre_end[counter] = DB_parser_copy.DB_constraints.DB_mere_end_mindre_end;
                                constraints.trafik_lys[counter] = DB_parser_copy.DB_constraints.DB_trafik_lys;
                                constraints.kommentar[counter] = DB_parser_copy.DB_constraints.DB_kommentar;

                                DB_parser_copy.DB_constraints.type = null;
                                DB_parser_copy.DB_constraints.DB_navn = null;
                                DB_parser_copy.DB_constraints.DB_req = false;
                                DB_parser_copy.DB_constraints.DB_dmean = double.NaN;
                                DB_parser_copy.DB_constraints.DB_rel_vol_dose = double.NaN;
                                DB_parser_copy.DB_constraints.DB_rel_vol = double.NaN;
                                DB_parser_copy.DB_constraints.DB_abs_vol_dose = double.NaN;
                                DB_parser_copy.DB_constraints.DB_abs_vol = double.NaN;
                                DB_parser_copy.DB_constraints.DB_mere_end_mindre_end = 0;
                                DB_parser_copy.DB_constraints.DB_trafik_lys = null;
                                DB_parser_copy.DB_constraints.DB_kommentar = null;
                            }

                            counter++;
                        }

                        for (int n = 0; n < prioritet.Count(); n++)
                        {

                            
                            switch (constraints.type[n])
                            {

                                
                                case "rapport":

                                    rapport(constraints.navn[n], context, patient, ss, plan, writer, constraints.trafik_lys[n]);
                                    continue;
                                case "dmean":
                                    dmean_check(constraints.navn[n], constraints.req[n], constraints.dmean[n], constraints.mere_end_mindre_end[n], constraints.trafik_lys[n], context, patient, ss, plan, writer);
                                    continue;

                                case "rel_vol":
                                    rel_vol_check(constraints.navn[n], constraints.req[n], constraints.rel_vol_dose[n], constraints.rel_vol[n], constraints.mere_end_mindre_end[n], constraints.trafik_lys[n], context, patient, ss, plan, writer);
                                    continue;

                                case "abs_vol":
                                    abs_vol_check(constraints.navn[n], constraints.req[n], constraints.abs_vol_dose[n], constraints.abs_vol[n], constraints.mere_end_mindre_end[n], constraints.trafik_lys[n], context, patient, ss, plan, writer);
                                    continue;

                                //case "manual":
                                //    manual(constraints.navn[n], constraints.req[n], constraints.kommentar[n], context);
                                //    continue;
                                default:
                                    continue;


                            }

                        }

                    }


                }

               
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        public static void getEverythingButDosis(ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string hospital)
        {
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
            try
            {


                string plan_name;
                if (context.PlanSetup.Id.ToLower().Substring(0, 2) == "cm")
                {
                    if (context.PlanSetup.Beams.First().IsocenterPosition.x < 0)
                    {
                        plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "h";
                    }
                    else
                    {
                        plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "v";
                    }

                }
                else if (context.PlanSetup.Id.Substring(0, 2).ToLower() == "hp" && context.PlanSetup.Id.ToLower().Contains("dib"))
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 8);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srk") //Der er 6 tegn i SRK koder, derfor skal de håndteres lidt anderledes
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 6);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srt")
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 3);
                }
                else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "gyn")
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 6);
                }
                else if (context.PlanSetup.Id.Length > 4)// resten af koderne har kun 5
                {
                    plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5);
                }
                else
                {
                    plan_name = context.PlanSetup.Id.ToLower() + "nnnnnnn";
                }

                if (plan_name.Substring(0, 2) != "cm" && plan_name.Substring(plan_name.Length - 3) == "000")// HVIS 000 kode så laver den en XX000_d hvor d er fraktionsdosis så der kan oprettes i diagnose tabel og prioritet et sæt for den situation
                {
                    plan_name = plan_name + "_" + context.PlanSetup.DosePerFraction.Dose.ToString();
                }
                else if (plan_name.Substring(0, 2) == "cm" && plan_name.Substring(plan_name.Length - 4, 3) == "000")
                {
                    plan_name = plan_name + "_" + context.PlanSetup.DosePerFraction.Dose.ToString();
                }

                DB_parser_copy get_diagnose_from_DB = new DB_parser_copy();

                bool DB_status = get_diagnose_from_DB.Read_diagnose(plan_name, hospital);
                int fx = 0;
                int diag_index = 0;
                double fx_dosis = 0;
                double norm_procent = 100;
                string norm_target = "ptv";
                string norm_volume = "dmean";
                string setup = null;
                if (!DB_status)
                {
                    //System.Windows.MessageBox.Show("Kunne ikke finde diagnosekode i database, check at plannavn er rigtigt: " + plan_name.ToUpper() + Environment.NewLine + "Forsætter med default værdier");

                }
                else
                {
                    diag_index = DB_parser_copy.DB_diag.DB_diag_code;
                    fx = DB_parser_copy.DB_diag.DB_fraktioner;
                    fx_dosis = DB_parser_copy.DB_diag.DB_fx_dosis;
                    norm_procent = DB_parser_copy.DB_diag.DB_norm_procent;
                    if (DB_parser_copy.DB_diag.DB_norm_target != null) { norm_target = DB_parser_copy.DB_diag.DB_norm_target; }
                    if (DB_parser_copy.DB_diag.DB_norm_volume != null) { norm_volume = DB_parser_copy.DB_diag.DB_norm_volume; }
                    setup = DB_parser_copy.DB_diag.DB_setup;
                }
                string trafik_lys = "r";
                string[] halcyon = { "Accelerator12","Accelerator11","Accelerator10" };
                //Plannavn og PlanID
                //Mål: Skal være det samme


                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Plan name == Plan Id", "", context.PlanSetup.Id, context.PlanSetup.Name, trafik_lys, plancheck_test.Plancheck_functions.plan_name_id(context));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "plan_name", plancheck_test.Plancheck_functions.plan_name_id(context), hospital);

                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Plan name == Plan Id", "", context.PlanSetup.Id, e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "plan_name", 3, hospital);
                }

                //Diagnosekode attached til course
                //Mål: Plannavn passer til kode attached
                string pattern = @"^[a-zA-Z][a-zA-Z][a-zA-Z]?[0-9][0-9][0-9]";
                string temp_name = Regex.Match(plan_name, pattern).Value.ToLower();
                try
                {
                    if (!context.Course.Diagnoses.Any(n => n.Code.ToLower().Contains(temp_name)))
                    {

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Diagnose attached to course", "", temp_name.ToUpper(), "Diagnose not attached to course", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "course_diagnose", 3, hospital);
                    }
                    else
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Diagnose attached to course", "", temp_name.ToUpper(), context.Course.Diagnoses.First(n => n.Code.ToLower().Contains(temp_name)).Code, trafik_lys, plancheck_test.Plancheck_functions.course_diagnose(temp_name, context));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "course_diagnose", plancheck_test.Plancheck_functions.course_diagnose(temp_name, context), hospital);
                    }
                }

                catch (Exception e)
                {

                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Diagnose attached to course", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "course_diagnose", 3, hospital);
                }

                //Antal fraktioner
                //Mål:
                //fx fra database
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Number of fractions", "", fx.ToString(), context.PlanSetup.NumberOfFractions.ToString(), trafik_lys, plancheck_test.Plancheck_functions.number_of_fractions(fx, context));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "number_fractions", plancheck_test.Plancheck_functions.number_of_fractions(fx, context), hospital);
                }
                catch (Exception e)
                {

                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Number of fractions", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "number_fractions", 3, hospital);
                }

                //Fraktionsdosis
                //Mål:
                //fx_dosis fra database
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose per fraction", "Gy", fx_dosis.ToString(), context.PlanSetup.DosePerFraction.Dose.ToString(), trafik_lys, plancheck_test.Plancheck_functions.dose_per_fraction(fx_dosis, context));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "fraction_dose", plancheck_test.Plancheck_functions.dose_per_fraction(fx_dosis, context), hospital);
                }
                catch (Exception e)
                {

                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose per fraction", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "fraction_dose", 3, hospital);
                }


                //Højdosis behandles med FFF
                //Mål: Patienter på FFF maskiner med fraktionsdosis på 5 Gy eller mere behandles med FFF
                bool fff = false;
                if (context.PlanSetup.Beams.First().TreatmentUnit.MachineModelName == "TDS")
                {
                    if (fx_dosis >= 5)
                    {
                        fff = true;
                    }
                }
                else if (context.PlanSetup.Beams.First().TreatmentUnit.MachineModelName == "RDS")
                {
                        fff = true;
                }
                try
                {
                    for (int n = 0; n < context.PlanSetup.Beams.Count(); n++)
                    {
                        if (context.PlanSetup.Beams.ElementAt(n).IsSetupField || context.PlanSetup.Beams.ElementAt(n).Technique.Id.ToLower().Contains("static") && context.PlanSetup.Beams.ElementAt(n).MLCPlanType.ToString().ToLower().Contains("static")) { continue; }
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Should fields be FFF", "",fff.ToString() , context.PlanSetup.Beams.ElementAt(n).Id+": "+context.PlanSetup.Beams.ElementAt(n).EnergyModeDisplayName , trafik_lys, plancheck_test.Plancheck_functions.fff(fff, context.PlanSetup.Beams.ElementAt(n)));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "is_fff", plancheck_test.Plancheck_functions.fff(fff, context.PlanSetup.Beams.ElementAt(n)), hospital);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Should fields be FFF", "", fff.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "is_fff", 3, hospital);
                }

                //FFF felter for HP behandlet i FB skal have lav dosisrate, alle andre max dosisrate
                //Mål: HP patienter behandlet i FB skal have doserate 600 for 6X og 800 for 10X. Alle andre skal have 1400 for 6X og 2400 for 10X.
                bool low_doserate = false;
                if (context.PlanSetup.Id.ToLower().Contains("hp") && context.Image.Id.ToLower().Contains("fb") || halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    low_doserate = true;
                }
                try
                {
                    if (fff)
                    {
                        for (int n = 0; n < context.PlanSetup.Beams.Count(); n++)
                        {

                            if (context.PlanSetup.Beams.ElementAt(n).IsSetupField || !context.PlanSetup.Beams.ElementAt(n).EnergyModeDisplayName.ToLower().Contains("fff")) { continue; }
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "FFF fields should have low doserate", "", context.PlanSetup.Beams.ElementAt(n).Id + ": "+low_doserate.ToString(), $"{ (context.PlanSetup.Beams.ElementAt(n).DoseRate <= 800 ? "True" : "False")}", trafik_lys, plancheck_test.Plancheck_functions.lowdoserate(low_doserate, context.PlanSetup.Beams.ElementAt(n)));
                            get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "Lowdoserate", plancheck_test.Plancheck_functions.lowdoserate(low_doserate, context.PlanSetup.Beams.ElementAt(n)), hospital);
                        }
                    }
                }
                catch(Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "FFF fields should have low doserate", "", low_doserate.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "Lowdoserate", 3, hospital);
                }
                

                //Ref punkt navn (nummer svarer til course)
                //Mål: X navn, X skal svare til CX som er course navn
                try
                {
                    int ref_number = Convert.ToInt16(context.Course.Id.Substring(1));

                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point number", "", ref_number.ToString(), context.PlanSetup.PrimaryReferencePoint.Id.Substring(0, ref_number.ToString().Length), trafik_lys, plancheck_test.Plancheck_functions.ref_point_number(ref_number, context.PlanSetup.PrimaryReferencePoint.Id.Substring(0, ref_number.ToString().Length)));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "reference_number", plancheck_test.Plancheck_functions.ref_point_number(ref_number, context.PlanSetup.PrimaryReferencePoint.Id.Substring(0, ref_number.ToString().Length)), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point number", "", ref_number.ToString(), e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "reference_number", 3, hospital);
                    }
                }
                catch (Exception)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point number", "", "", "Reference point name does not start with a number", trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "reference_number", 3, hospital);
                }


                //Den med 9 : Resp 2.0 B30f 0 - 90 Pi TRIGGER_DELAY 20 %
                // Den med 7: Resp 2.0 B30f 0 - 90 Pi TRIGGER_DELAY 50 %
                //Check at scan dato er samme dato som scanning er navngivet og at scan dato er inden for de sidste 14 dage

                //Scan navngivning
                //Mål:
                DateTime scan_date = context.PlanSetup.Series.Study.CreationDateTime.GetValueOrDefault(new DateTime(1980, 12, 16)); //Skal være ddMMyy hvis ikke 4D eller lunge DIBH
                string target_scan_name = scan_date.Date.ToString("ddMMyy");
                
                if (context.Image.Series.Comment.Contains("Resp 2.0 B30f")) //4D 
                {
                    target_scan_name = "FB planlægning";
                }
                else if ((context.Image.Series.Comment.ToLower().Contains("dibh") || context.Image.Series.Comment.ToLower().Contains("breathhold")) && !plan_name.Contains("cm") && !plan_name.Contains("hn")) // DIBH OG ikke mam
                {
                    target_scan_name = "DIBH planlægning";
                }
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan name", "", target_scan_name, context.Image.Id, trafik_lys, plancheck_test.Plancheck_functions.scan_name(target_scan_name, context));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "scan_name", plancheck_test.Plancheck_functions.scan_name(target_scan_name, context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan name", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "scan_name", 3, hospital);
                }

                //DIBH scan navn
                //Mål: Hvis plan burde være DIBH, fra database (DIBH = 1)


                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan name, DIBH", "", $"{(DB_parser_copy.DB_diag.DB_DIBH==1 ? "DIBH" : "not DIBH")}", context.Image.Id, trafik_lys, plancheck_test.Plancheck_functions.scan_dibh( context.Image.Id.ToLower(), DB_parser_copy.DB_diag.DB_DIBH));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"scan_DIBH",  plancheck_test.Plancheck_functions.scan_dibh( context.Image.Id.ToLower(), DB_parser_copy.DB_diag.DB_DIBH), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan name, DIBH", "", "", e.ToString(), "", 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"scan_DIBH", 3, hospital);
                    }

                

                //Use gated
                //Mål skal være true hvis plan billede er DIBH
                if (context.Image.Id.ToLower().Contains("dibh"))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Use gated", "", "True", context.ExternalPlanSetup.UseGating.ToString(), trafik_lys, plancheck_test.Plancheck_functions.use_gated(context.ExternalPlanSetup.UseGating, true));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "use_gated", plancheck_test.Plancheck_functions.use_gated(context.ExternalPlanSetup.UseGating, true), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Use gated", "", "True", e.ToString(), "", 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "use_gated", 3, hospital);

                    }
                }
                else
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Use gated", "", "False", context.ExternalPlanSetup.UseGating.ToString(), trafik_lys, plancheck_test.Plancheck_functions.use_gated(context.ExternalPlanSetup.UseGating, false));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "use_gated", plancheck_test.Plancheck_functions.use_gated(context.ExternalPlanSetup.UseGating, false), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Use gated", "", "False", e.ToString(), "", 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "use_gated", 3, hospital);

                    }
                }



                //Scan dato
                //Mål: Inden for de sidste 14 dage

                DateTime target_date = Convert.ToDateTime(context.Image.CreationDateTime);
                target_date = target_date.AddDays(-14);
                try
                {

                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan date, recent", "", target_date.Date.ToString("ddMMyy"), context.Image.CreationDateTime.Value.Date.ToString("ddMMyy"), trafik_lys, plancheck_test.Plancheck_functions.scan_date(target_date, context));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "scan_date", plancheck_test.Plancheck_functions.scan_date(target_date, context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Scan date, recent", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "scan_date", 3, hospital);
                }


                //User origin i body
                //Mål: User origin bør være i body
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "User origin in Body", "", "True", context.StructureSet.Structures.First(n => n.Id.ToLower().Equals("body")).IsPointInsideSegment(context.Image.UserOrigin).ToString(), trafik_lys, plancheck_test.Plancheck_functions.User_origin_in_body(context.Image.UserOrigin, context.StructureSet.Structures.First(n => n.Id.ToLower().Equals("body"))));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "user_origin_in_body", plancheck_test.Plancheck_functions.User_origin_in_body(context.Image.UserOrigin, context.StructureSet.Structures.First(n => n.Id.ToLower().Equals("body"))), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "User origin in Body", "", "True", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "user_origin_in_body", 3, hospital);
                }


                //Lejetop type
                //Mål: Brainlab i beam couch, alle undtagen HH. Exact IGRT Couch, medium på 7 og 8
                string target_couch = @"BrainLAB/iBeam Couch";
                string[] varian_couch = { "Accelerator14","Accelerator07", "Accelerator08", "Accelerator04", "Accelerator03", "Accelerator02" };
                
                bool couch_needed = false;
                var no_couch = new List<string> { "hh", "cn", "srk", "srt", "lu008", "tb" };
                if (varian_couch.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    target_couch = @"Exact IGRT Couch, medium";
                }
                else if (halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    couch_needed = true;
                    target_couch = @"Halcyon Couch";
                }
                Structure couch_int = context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).FirstOrDefault();
                Structure couch_ext = context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchSurface")).FirstOrDefault();
                
                try
                {

                    if (no_couch.Any(plan_name.ToLower().Contains))
                    {
                        if (context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).FirstOrDefault() == null)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", "None", "None", trafik_lys, 1);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_type", 1, hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", "None", context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).First().Name, trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_type", 3, hospital);
                        }
                    }
                    else
                    {
                        couch_needed = true;
                        if (couch_int != null)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", target_couch, context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).First().Name, trafik_lys, plancheck_test.Plancheck_functions.couch_type(target_couch, context));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_type", plancheck_test.Plancheck_functions.couch_type(target_couch, context), hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", "Brainlab couch", "Couch missing from plan", trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_type", 3, hospital);
                        }

                    }

                }
                catch (Exception e)
                {
                    //  VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", target_couch, context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).FirstOrDefault().Name, trafik_lys, 3);
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch top", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_type", 3, hospital);
                }


                //Lejetop HU
                //Mål, 1-6, 10-14: -820 og -300. 7 og 8: -1000 og -300

                int couch_internal_HU_target = -820;
                int couch_external_HU_target = -300;
                if (varian_couch.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id) || halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    couch_internal_HU_target = -1000;
                    couch_external_HU_target = -300;
                }



                try
                {
                    if (couch_needed)
                    {


                        if (couch_int != null)
                        {
                            double couch_internal_HU;
                            double couch_external_HU;
                            couch_int.GetAssignedHU(out couch_internal_HU);
                            couch_ext.GetAssignedHU(out couch_external_HU);

                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch internal HU", "HU", couch_internal_HU_target.ToString(), Convert.ToInt16(couch_internal_HU).ToString(), trafik_lys, plancheck_test.Plancheck_functions.couch_HU(couch_internal_HU_target, Convert.ToInt16(couch_internal_HU)));
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch external HU", "HU", couch_external_HU_target.ToString(), Convert.ToInt16(couch_external_HU).ToString(), trafik_lys, plancheck_test.Plancheck_functions.couch_HU(couch_external_HU_target, Convert.ToInt16(couch_external_HU)));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_HU_int", plancheck_test.Plancheck_functions.couch_HU(couch_internal_HU_target, Convert.ToInt16(couch_internal_HU)), hospital);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_HU_ext", plancheck_test.Plancheck_functions.couch_HU(couch_external_HU_target, Convert.ToInt16(couch_external_HU)), hospital);
                        }

                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch internal HU", "", "", "No couch detected", "", 3);
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Couch internal HU", "", "", e.ToString(), "", 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_HU_int", 3, hospital);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "couch_HU_ext", 3, hospital);
                }

                //Klatter i CTV
                //ikke for HH og CM
                string klat_CTV = "";
                int klat_CTV_status = 1;
                if (!context.PlanSetup.Id.Substring(0, 2).ToLower().Contains("hh") && !context.PlanSetup.Id.Substring(0, 2).ToLower().Contains("cm"))
                {
                    try
                    {
                        foreach (var contour in context.StructureSet.Structures.Where(n => n.Id.Length>2 && (n.Id.ToLower().Substring(0, 3).Contains("ctv") || n.Id.ToLower().Substring(0, 3).Contains("gtv")) && !n.IsEmpty))
                        {
                            if (plancheck_test.Plancheck_functions.CTV_klat(contour) == 3)
                            {
                                klat_CTV = klat_CTV + contour.Id + System.Environment.NewLine;
                                klat_CTV_status = 3;
                            }

                        }
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "CTV multiple structures", "", "1", klat_CTV, trafik_lys, klat_CTV_status);
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "CTV_klat", klat_CTV_status, hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "CTV klat", "", "", "No CTV detected", "", 3);
                    }
                }




                //Virtuel bolus
                //Mål: Attached til alle felter
                int virtual_boluses = context.StructureSet.Structures.Where(n => n.DicomType.ToLower().Contains("bolus")).Count();

                try
                {
                    bool bolus = true;
                    foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false))
                    {
                        if (plancheck_test.Plancheck_functions.virtual_bolus(virtual_boluses, beam) == 3)
                        {
                            bolus = false;
                        }
                    }
                    if (bolus)
                    {

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Virtual bolus attached to all fields", "", $"{virtual_boluses} boluses", "All boluses correctly connected", trafik_lys, 1);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "virtual_bolus", 1, hospital);
                    }
                    else
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Virtual bolus attached to all fields", "", $"{virtual_boluses} boluses", $"{context.PlanSetup.Beams.First(n=> n.Boluses.Count()!=virtual_boluses).Id} is missing bolus", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "virtual_bolus", 3, hospital);
                    }
                    

                }
                catch (Exception e)
                {
                    foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false))
                    {
                        // VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Virtual bolus attached to all fields", "", virtual_boluses.ToString(), beam.Id + ": " + beam.Boluses.Count().ToString(), trafik_lys, 3);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Virtual bolus attached to all fields", "", "", e.ToString(), "", 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "virtual_bolus", 3, hospital);
                    }
                }


                //Samme isocenter
                //Mål: Alle felter bør have samme isocenter, ikke TBI
                bool is_tbi = context.PlanSetup.Id.Substring(0,2).ToLower().Contains("tb");
                double iso_x = context.PlanSetup.Beams.First().IsocenterPosition.x;
                double iso_y = context.PlanSetup.Beams.First().IsocenterPosition.y;
                double iso_z = context.PlanSetup.Beams.First().IsocenterPosition.z;
                if (!is_tbi)
                {
                    try
                    {
                        if (plancheck_test.Plancheck_functions.same_isocenter(iso_x, iso_y, iso_z, context) == 3)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter", "", "Yes", context.PlanSetup.Beams.Where(n => n.IsocenterPosition.x != iso_x || n.IsocenterPosition.y != iso_y || n.IsocenterPosition.z != iso_z).FirstOrDefault().Id, trafik_lys, plancheck_test.Plancheck_functions.same_isocenter(iso_x, iso_y, iso_z, context));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", plancheck_test.Plancheck_functions.same_isocenter(iso_x, iso_y, iso_z, context), hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter", "", "Yes", "Yes", trafik_lys, plancheck_test.Plancheck_functions.same_isocenter(iso_x, iso_y, iso_z, context));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", plancheck_test.Plancheck_functions.same_isocenter(iso_x, iso_y, iso_z, context), hospital);
                        }
                    }
                    catch (Exception e)
                    {

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter", "", "Yes", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", 3, hospital);
                    }
                }
                else
                {
                    try
                    {
                        if (plancheck_test.Plancheck_functions.same_isocenter_tbi(iso_y, iso_z, context) == 3)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter (Y and Z)", "", "Yes", context.PlanSetup.Beams.Where(n => n.IsocenterPosition.y != iso_y || n.IsocenterPosition.z != iso_z).FirstOrDefault().Id, trafik_lys, plancheck_test.Plancheck_functions.same_isocenter_tbi(iso_y, iso_z, context));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", plancheck_test.Plancheck_functions.same_isocenter_tbi(iso_y, iso_z, context), hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter (Y and Z)", "", "Yes", "Yes", trafik_lys, plancheck_test.Plancheck_functions.same_isocenter_tbi(iso_y, iso_z, context));
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", plancheck_test.Plancheck_functions.same_isocenter_tbi(iso_y, iso_z, context), hospital);
                        }
                    }
                    catch (Exception e)
                    {

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Same isocenter (Y and Z)", "", "Yes", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "same_isocenter", 3, hospital);
                    }
                }

                //TBI X isocenter +- 250
                if (is_tbi)
                {
                    try
                    {
                        iso_x = 2500;
                        
                        foreach (var beam in context.PlanSetup.Beams)
                        {
                            double x = context.Image.UserOrigin.x - beam.IsocenterPosition.x;


                         
                                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "X isocenter", "cm", "+/- 250", $"{beam.Id} has x={x/10}", trafik_lys, plancheck_test.Plancheck_functions.x_isocenter(iso_x, x));
                                get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "x_isocenter", plancheck_test.Plancheck_functions.x_isocenter(iso_x, x), hospital);
                           
                        }

                    }

                    catch (Exception e)
                    {

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "X isocenter", "", "Yes", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "x_isocenter", 3, hospital);
                    }
                }
            

                //Felt navne nummereret korrekt
                //Mål: 1 feltnavn, 2 feltnavn hvis <10 felter, 01 feltnavn, 02 feltnavn >=10 felter
                //Mål: Statiske setup felter inderholder gantry vinkel
                //Mål: TBI: Field 01....
                int field_name_digits;
                string field_name_format;
                string setup_string = "";
          
                if (context.PlanSetup.Beams.Where(m => m.IsSetupField == false).Count() < 10)
                {
                    field_name_format = "1 Beam";
                    field_name_digits = 1;
                }
                else if (context.PlanSetup.Id.Substring(0,2).ToLower().Contains("tb"))
                {
                    field_name_format = "Field 01";
                    is_tbi = true;
                    field_name_digits = 2;
                    

                }
                else
                {
                    field_name_format = "01 Beam";
                    field_name_digits = 2;
                }

                var obi_fields = context.PlanSetup.Beams.Where(n => n.IsSetupField == true && !n.Id.ToLower().Contains("cbct") && !n.Id.ToLower().Contains("pvi"));

                foreach (var beam in obi_fields)
                {
                    try
                    {
                        if (is_tbi)
                        {
                            setup_string = (Math.Round(beam.ControlPoints.First().GantryAngle) == 275) ? "R":"L";
                        }
                        else
                        {
                            setup_string = " " + beam.ControlPoints.First().GantryAngle.ToString();
                        }

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup field name", "", "Setup" + setup_string, beam.Id, trafik_lys, plancheck_test.Plancheck_functions.setup_name(beam,setup_string));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", plancheck_test.Plancheck_functions.setup_name(beam,setup_string), hospital);
                    }
                    catch (Exception e)
                    {
                        //VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup field name", "", "Setup " + beam.ControlPoints.First().GantryAngle.ToString(), beam.Id, trafik_lys, 3);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup field name", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", 3, hospital);
                    }
                }

                if (plan_name.Contains("cm060"))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "CBCT for CM060", "", "CBCT", context.PlanSetup.Beams.Where(n => n.IsSetupField == true && n.Id.ToLower().Contains("cbct")).First().Id, trafik_lys, 1);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", 1, hospital);
                    }

                    catch (Exception e)
                    {
                        //VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup PVI for CM060", "", "Setup pvi", "none", trafik_lys, 3);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "CBCT for CM060", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", 3, hospital);
                    }
                }
                

                bool treatment_name_fail = false;
                int beam_counter = 1;
                foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false))
                {
                    string beam_number = beam_counter.ToString();
                    if (field_name_digits == 2)
                    {
                        beam_number = beam_counter.ToString("D2");
                    }

                    if (plancheck_test.Plancheck_functions.treat_name(beam, field_name_digits, is_tbi) == 3)
                    {
                        treatment_name_fail = true;
                        try
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment field name", "", beam_number + " " + Regex.Replace(beam.Id, @"^[\d-]*\s*", ""), beam.Id, trafik_lys, plancheck_test.Plancheck_functions.treat_name(beam, field_name_digits, is_tbi));
                            get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", plancheck_test.Plancheck_functions.treat_name(beam, field_name_digits, is_tbi), hospital);
                        }

                        catch (Exception e)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment field name", "", e.ToString(), "", trafik_lys, 3);
                            get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", 3, hospital);
                        }
                    }
                    beam_counter++;

                }
                if (!treatment_name_fail)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment field name", "", field_name_format, "All fields", trafik_lys, 1);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"field_names", 1, hospital);
                }

                //Spoiler
                //Der skal være spoiler for TBI, de skal være med i body og de skal have material water
                if (is_tbi)
                {
                    try
                    {
                        Structure spoiler_r = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_r"));
                        Structure spoiler_l = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_l"));

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Left spoiler exists", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_l) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_l));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_exists", plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_l), hospital);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Right spoiler exists", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_r) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_r));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_exists", plancheck_test.Plancheck_functions.tbi_spoiler_exist(spoiler_r), hospital);

                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Spoiler exists", "", e.ToString(), "", trafik_lys, 3);
                    }
                    try
                    {
                        Structure spoiler_r = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_r"));
                        Structure spoiler_l = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_l"));

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Left spoiler included in body", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_l, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_l, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_in_body", plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_l, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))), hospital);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Right spoiler included in body", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_r, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_r, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_in_body", plancheck_test.Plancheck_functions.tbi_spoiler_in_body(spoiler_r, context.StructureSet.Structures.First(n => n.StructureCodeInfos.First().Code.Equals("BODY"))), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Spoiler included in body", "", e.ToString(), "", trafik_lys, 3);
                    }
                    try
                    {
                        Structure spoiler_r = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_r"));
                        Structure spoiler_l = context.StructureSet.Structures.FirstOrDefault(n => n.Id.ToLower().Contains("spoiler_l"));

                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Left spoiler is water", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_l) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_l));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_water", plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_l), hospital);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Right spoiler is water", "", "True", (plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_r) == 1) ? "True" : "False", trafik_lys, plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_r));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "spoiler_water", plancheck_test.Plancheck_functions.tbi_spoiler_water(spoiler_r), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Spoiler is water", "", e.ToString(), "", trafik_lys, 3);
                    }
                    

                }

                //Kiler på DIBH plan
                //Mål: Ingen kiler på DIBH planer
                bool DIBH_wedge_fail = false;
                foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false))
                {
                    try
                    {
                        if (plancheck_test.Plancheck_functions.DIBH_wedge(beam, context) == 3)
                        {
                            DIBH_wedge_fail = true;
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Wedge on DIBH plans", "", "No wedges on DIBH", beam.Id, trafik_lys, plancheck_test.Plancheck_functions.DIBH_wedge(beam, context));
                            get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"DIBH_wedge", plancheck_test.Plancheck_functions.DIBH_wedge(beam, context), hospital);
                        }

                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Wedge on DIBH plans", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"DIBH_wedge", 3, hospital);
                    }
                }
                if (!DIBH_wedge_fail && (context.Image.Comment.ToLower().Contains("dibh") || context.Image.Comment.ToLower().Contains("breathhold")))
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Wedge on DIBH plans", "", "No wedges on DIBH", "", trafik_lys, 1);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"DIBH_wedge", 1, hospital);
                }


                //Isocenter for rapidarc
                //Mål: Ingen buer over midten hvis isocenter uden for tolerance
                //Hvis der er couch bruges couch til at bestemme midte. Hvis der ikke er couch bruges kun couch kick.
                if (couch_int != null && context.PlanSetup.Beams.Any(n=> n.Technique.Id.ToLower().Contains("arc")))
                {
                    double mid_x = context.StructureSet.Structures.First(n => n.Id.ToLower().Contains("couchsurface")).MeshGeometry.Bounds.X + context.StructureSet.Structures.First(n => n.Id.ToLower().Contains("couchsurface")).MeshGeometry.Bounds.SizeX / 2;
                    double top_y = context.StructureSet.Structures.First(n => n.Id.ToLower().Contains("couchsurface")).MeshGeometry.Bounds.Y;// + context.StructureSet.Structures.First(n => n.Id.ToLower().Contains("couchsurface")).MeshGeometry.Bounds.SizeY;
                    double x = context.PlanSetup.Beams.First().IsocenterPosition.x - mid_x;
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Arc plan collision danger due to isocenter position", "", "No","",  trafik_lys, plancheck_test.Plancheck_functions.arc_collision(x,top_y, context.PlanSetup.Beams.First().IsocenterPosition.y,context.PlanSetup.Beams,mid_x));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "arc_collision", plancheck_test.Plancheck_functions.arc_collision(x, top_y, context.PlanSetup.Beams.First().IsocenterPosition.y, context.PlanSetup.Beams, mid_x), hospital);              
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Arc plan collision danger due to isocenter position", "", "No", e.ToString(), trafik_lys, 1);
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "arc_collision", 1, hospital);
                    }
                }

                //Rapidarc felter collimator
                //Mål: ingen collimator i kardinal vinkler. Ingen felter med samme collimator vinkel med mindre der er forskellig leje vinkel

                var arcs = context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc"));
                bool arc_cardinal_fail = false;
                foreach (var beam in context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc")))
                {
                    try
                    {
                        if (plancheck_test.Plancheck_functions.arc_cardinal(beam) == 3)
                        {
                            arc_cardinal_fail = true;
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Collimator in cardinal directions", "", "None", beam.Id, trafik_lys, plancheck_test.Plancheck_functions.arc_cardinal(beam));
                            get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", plancheck_test.Plancheck_functions.arc_cardinal(beam), hospital);
                        }

                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Collimator in cardinal directions", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", 3, hospital);
                    }
                }
                if (!arc_cardinal_fail && context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc")).Count() > 0)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Collimator in cardinal directions", "", "None", "", trafik_lys, 1);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", 1, hospital);
                }

                bool arc_overlapping_collimator_fail = false;
                foreach (var beam in context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc")))
                {
                    try
                    {
                        if (plancheck_test.Plancheck_functions.arc_overlap(beam, context) == 3)
                        {
                            arc_overlapping_collimator_fail = true;
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No identical collimator angles", "", "None", beam.Id, trafik_lys, plancheck_test.Plancheck_functions.arc_overlap(beam, context));
                            get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", plancheck_test.Plancheck_functions.arc_overlap(beam, context), hospital);
                        }

                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No identical collimator angles", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", 3, hospital);
                    }
                }
                if (!arc_overlapping_collimator_fail && context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc")).Count() > 0)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No identical collimator angles", "", "None", "", trafik_lys, 1);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_collimator", 1, hospital);
                }

                //TBI collimator vinkler
                //Mål: Må ikke være mellem 160 og 200, og ikke mellem 340 og 20
                if (is_tbi)
                {
                    try
                    {
                        string bad_field = plancheck_test.Plancheck_functions.tbi_coll_angle(context.PlanSetup.Beams);
                        if (!Equals(bad_field, null))
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No collimator angles between 160-200 and 340-20", "", "None", context.PlanSetup.Beams.First(n => n.ControlPoints.First().CollimatorAngle < 20 || n.ControlPoints.First().CollimatorAngle < 200 && n.ControlPoints.First().CollimatorAngle > 160 || n.ControlPoints.First().CollimatorAngle > 340).Id, trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "TBI_collimator", 3, hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No collimator angles between 160-200 and 340-20", "", "None", "None", trafik_lys, 1);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "TBI_collimator", 1, hospital);
                        }
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "No collimator angles between 160-200 and 340-20", "", "None", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "TBI_collimator", 3, hospital);
                    }
                }

                //Arc MLC som er helt ude i 15 cm
                //Mål: <1% af samlet antal MLC kontrolpunkter må være i 15 cm (Check om felter er lukket, i så tilfælde fint)
                //Ligegyldig for TBI
                if (!is_tbi && !halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    double fraction_of_mlc_at_max = 1;

                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MLC leafs at maximum extension", "%", "<1", Math.Round(plancheck_test.Plancheck_functions.mlc_at_max_(context), 2).ToString(), trafik_lys, plancheck_test.Plancheck_functions.mlc_at_max(plancheck_test.Plancheck_functions.mlc_at_max_(context), fraction_of_mlc_at_max));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "MLC_max_extension", plancheck_test.Plancheck_functions.mlc_at_max(plancheck_test.Plancheck_functions.mlc_at_max_(context), fraction_of_mlc_at_max), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MLC leafs at maximum extension", "%", "<1", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "MLC_max_extension", 3, hospital);
                    }
                }

                //MLC ikke ved kæbe kanten
                //Mål: Ingen MLC blade mindre end 8 mm fra X kæbe, 2 mm for IMRT og VMAT.(Check om felter er lukket, i så tilfælde fint)
                //Lige gyldig for TBI
                if (!is_tbi & !halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    double static_mlc_limit = 8;
                    double dyn_mlc_limit = 2;
                    try
                    {
                        double distance = 0;
                        double fraction = 0;
                        int eval = plancheck_test.Plancheck_functions.mlc_at_jaw(context.PlanSetup.Beams, context.PlanSetup.Beams.First().MLC.Model, static_mlc_limit, dyn_mlc_limit, out distance, out fraction);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Minimum MLC leaf distance to X-jaw", "", "2 mm for VMAT, 8 mm for static", "Fraction of MLCs below tolerance: " + fraction.ToString() + "%. Minimum distance: " + distance.ToString() + "mm", trafik_lys, eval);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Minimum MLC leaf distance to X-jaw", "", "2 mm for VMAT, 8 mm for static", e.ToString(), trafik_lys, 3);
                    }
                }

                //Arc x collimator
                //Mål: <= 21 cm
                double x_collimator_limit = 21;


                foreach (var beam in context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc") && !halcyon.Contains(n.TreatmentUnit.Id)))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "X-collimator <=" + x_collimator_limit.ToString() + "cm", "cm", "<=" + x_collimator_limit.ToString(), Math.Round(-beam.ControlPoints.First().JawPositions.X1 / 10 + beam.ControlPoints.First().JawPositions.X2 / 10, 1).ToString(), trafik_lys, plancheck_test.Plancheck_functions.arc_x_coll(beam, x_collimator_limit));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_x_collimator", plancheck_test.Plancheck_functions.arc_x_coll(beam, x_collimator_limit), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "X-collimator <=" + x_collimator_limit.ToString() + "cm", "cm", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"RA_x_collimator", 3, hospital);
                    }
                }

                //Y collimator
                //Mål: HCMLC: 10.8, Millenium 19.9 cm
                double y_collimator_limit = 19.9;



                if (context.PlanSetup.Beams.First(n => n.IsSetupField == false).MLC.Id.Equals("mlchd120", StringComparison.OrdinalIgnoreCase))
                {
                    y_collimator_limit = 10.8;
                }

                foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false && !halcyon.Contains(n.TreatmentUnit.Id)))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Y-collimator <=" + y_collimator_limit.ToString() + "cm", "cm", "<=" + y_collimator_limit.ToString(), "Y1: " + (beam.ControlPoints.First().JawPositions.Y1 / 10).ToString() + " Y2: " + (beam.ControlPoints.First().JawPositions.Y2 / 10).ToString(), trafik_lys, plancheck_test.Plancheck_functions.y_coll(beam, y_collimator_limit));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"y_collimator", plancheck_test.Plancheck_functions.y_coll(beam, y_collimator_limit), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Y-collimator <=" + y_collimator_limit.ToString() + "cm", "cm", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"y_collimator", 3, hospital);
                    }
                }

                //TBI gantry vinkler
                //Mål: 275 og 85
                if (is_tbi)
                {
                    double gantry_tbi_L = 85;
                    double gantry_tbi_R = 275;

                    foreach (var beam in context.PlanSetup.Beams)
                    {
                        try
                        {
                            if (beam.IsocenterPosition.x < 0)
                            {
                                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "TBI gantry angle", "", gantry_tbi_R.ToString(), $"{beam.Id} has gantry angle {beam.ControlPoints.First().GantryAngle}", trafik_lys, plancheck_test.Plancheck_functions.tbi_gantry_angle(beam, gantry_tbi_R));
                                get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_gantry", plancheck_test.Plancheck_functions.tbi_gantry_angle(beam, gantry_tbi_R), hospital);
                            }
                            else
                            {
                                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "TBI gantry angle", "", gantry_tbi_L.ToString(), $"{beam.Id} has gantry angle {beam.ControlPoints.First().GantryAngle}", trafik_lys, plancheck_test.Plancheck_functions.tbi_gantry_angle(beam, gantry_tbi_L));
                                get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_gantry", plancheck_test.Plancheck_functions.tbi_gantry_angle(beam, gantry_tbi_L), hospital);
                            }
                        }
                        catch (Exception e)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "TBI gantry angle", "", $"{gantry_tbi_L} for left side, {gantry_tbi_R} for right side", e.ToString(), trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_gantry", 3, hospital);
                        }
                    }
                }

                

               

                //Setup felter
                //Mål: kæber 18 cm
                double setup_collimator = 18;
                if (!is_tbi)
                {
                    var setup_fields = context.PlanSetup.Beams.Where(n => n.IsSetupField == true && !n.Id.Contains("pvi") && !halcyon.Contains(n.TreatmentUnit.Id));
                    foreach (var beam in setup_fields)
                    {
                        try
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup fields: " + setup_collimator.ToString() + "x" + setup_collimator.ToString() + "cm", "cm", setup_collimator.ToString(), "X: " + (-beam.ControlPoints.First().JawPositions.X1 / 10 + beam.ControlPoints.First().JawPositions.X2 / 10).ToString() + " Y: " + (-beam.ControlPoints.First().JawPositions.Y1 / 10 + beam.ControlPoints.First().JawPositions.Y2 / 10).ToString(), trafik_lys, plancheck_test.Plancheck_functions.setup_coll(beam, setup_collimator));
                            get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "setup_jaws", plancheck_test.Plancheck_functions.setup_coll(beam, setup_collimator), hospital);
                        }
                        catch (Exception e)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Setup fields: " + setup_collimator.ToString() + "x" + setup_collimator.ToString() + "cm", "cm", setup_collimator.ToString(), e.ToString(), trafik_lys, 3);
                            get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "setup_jaws", 3, hospital);
                        }

                    }
                }

                //Bones indtegnet på CBCT
                //Mål: Alle med CBCT felt skal have Bone struktur som er tegnet

                if (context.PlanSetup.Beams.Any(n => n.IsSetupField == true && n.Id.ToLower().Contains("cbct")))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Bone contours for CBCT", "", "Bone contours", "", trafik_lys, plancheck_test.Plancheck_functions.cbct_bones(context));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"bones_CBCT", plancheck_test.Plancheck_functions.cbct_bones(context), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Bone contours for CBCT", "", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"bones_CBCT", 3, hospital);
                    }
                }

                //OBI feltvinkler
                //Mål: targets med isocenter i minus OBI i 270 og 180 (højre side for HFS). Isocenter i plus, OBI i 0 og 180
                double obi_target_ap = 0;
                double obi_target_lat = 270;
                if (!is_tbi && context.PlanSetup.Beams.Count(n => n.IsSetupField == true && !n.Id.ToLower().Contains("cbct")) > 1)
                {
                    if (context.PlanSetup.Beams.First().IsocenterPosition.x < 0)
                    {
                        obi_target_ap = 180;
                    }

                    try
                    {
                        string obi_ap_id;
                        int valuation = plancheck_test.Plancheck_functions.obi_angle(obi_target_ap, context, out obi_ap_id);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "OBI gantry angles", "°", obi_target_ap.ToString(), obi_ap_id, trafik_lys, valuation);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"obi_angles", valuation, hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "OBI gantry angles", "°", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"obi_angles", 3, hospital);
                    }
                    try
                    {
                        string obi_lat_id;
                        int valuation = plancheck_test.Plancheck_functions.obi_angle(obi_target_lat, context, out obi_lat_id);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "OBI gantry angles", "°", obi_target_lat.ToString(), obi_lat_id, trafik_lys, valuation);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"obi_angles", valuation, hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "OBI gantry angles", "°", e.ToString(), "", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"obi_angles", 3, hospital);
                    }
                }

                //Extended CBCT
                //Mål: Felt større end 18 cm i Y retning
                //Hvis TB skal der extended CBCT og hvis Clinac skal der CBCT + OBI
                if (!is_tbi && !plan_name.Contains("cm") && context.PlanSetup.Beams.Where(n=> n.MLCPlanType.ToString().Equals("DoseDynamic")|| n.MLCPlanType.ToString().Equals("VMAT")).Count()>0)
                {
                    double target = 180;
                    if (halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                    {
                        target = 240;
                    }
                    string strategy = null;
                    switch (context.PlanSetup.Beams.First().TreatmentUnit.MachineModel.ToLower())
                    {
                        case "tds":
                            strategy = "Write in document, Behandlingsforløb: 'Brug extended CBCT'";
                            break;
                        default:
                            if (plan_name.Contains("hn"))
                            {
                                strategy = "PT should be treated on Truebeam with extended CBCT";
                            }
                            else
                            {
                                strategy = "Write in document, Behandlingsforløb: 'Brug CBCT + OBI'";
                            }
                            break;
                    }

                    try
                    {
                        if (context.StructureSet.Structures.First(n=> n.Id == context.PlanSetup.OptimizationSetup.Objectives.Where(a=> a.Operator.ToString().Equals("Lower")).OrderByDescending(m=> m.Structure.MeshGeometry.Bounds.SizeZ).First().StructureId).MeshGeometry.Bounds.SizeZ > target)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, $"Is extended imaging needed? (PTV > {target/10} cm)", "", "Yes", strategy, trafik_lys, 6);
                            get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "extended_imaging", 1, hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, $"Is extended imaging needed? (PTV > {target / 10} cm)", "", "No", "No extended imaging", trafik_lys, 1);
                            get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "extended_imaging", 1, hospital);
                        }
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, $"Is extended imaging needed? (PTV > {target / 10} cm)", "", "True", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "extended_imaging", 3, hospital);
                    }
                }

                //Algoritme valg
                //Mål: Alle skal have Acuros, bortset fra TBI som skal have AAA. Elektroner skal have Genralized pencil beam version 11
                string algorithm;

                if (plan_name.Substring(0, 2) == "tb")
                {
                    algorithm = "AAA_16.1.0";
                }
                else if (context.PlanSetup.Beams.Any(n => n.EnergyModeDisplayName.ToLower().Contains("e")))
                {
                    algorithm = "EMC_16.1.0";
                }
                else
                {
                    algorithm = "AcurosXB_16.1.0";
                }
                if (halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    algorithm = "AcurosXB_HAL_16.1.0";
                }

                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation algorithm", "", algorithm, context.PlanSetup.PhotonCalculationModel, trafik_lys, plancheck_test.Plancheck_functions.dose_algorithm(algorithm, context));
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"dose_algorithm", plancheck_test.Plancheck_functions.dose_algorithm(algorithm, context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation algorithm", "", algorithm, e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"dose_algorithm", 3, hospital);
                }

                //Grid size
                //Mål: Stereotaksier og højdosis cranier: 1x1x1 mm, andre fotoner: 2.5x2.5x2 mm, elektroner 1.25x1.25x1.25?
                double dose_x_resolution;
                double dose_y_resolution;
                double dose_z_resolution;

                if (plan_name.Contains("srk") || plan_name.Contains("srt") || plan_name.Contains("mt016") || plan_name.Contains("hp") || plan_name.Contains("cn004") || plan_name.Contains("cn002") || plan_name.Contains("cn006"))
                {
                    dose_x_resolution = 1;
                    dose_y_resolution = 1;
                    dose_z_resolution = 1;
                }
                else if (context.PlanSetup.Beams.Any(n => n.EnergyModeDisplayName.ToLower().Contains("e")))
                {
                    dose_x_resolution = 1.25;
                    dose_y_resolution = 1.25;
                    dose_z_resolution = 1.25;
                }
                else if (plan_name.Contains("srt"))
                {
                    dose_x_resolution = 1.5;
                    dose_y_resolution = 1.5;
                    dose_z_resolution = 1.5;
                }
                else
                {
                    dose_x_resolution = 2.5;
                    dose_y_resolution = 2.5;
                    dose_z_resolution = 2;
                }

                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation resolution", "mm", dose_x_resolution.ToString() + "x" + dose_y_resolution.ToString() + "x" + dose_z_resolution.ToString(), Math.Round(context.PlanSetup.Dose.XRes, 2).ToString() + "x" + Math.Round(context.PlanSetup.Dose.YRes, 2).ToString() + "x" + Math.Round(context.PlanSetup.Dose.ZRes, 2).ToString(), trafik_lys, plancheck_test.Plancheck_functions.dose_resolution(dose_x_resolution, dose_y_resolution, dose_z_resolution, context));
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"grid_size", plancheck_test.Plancheck_functions.dose_resolution(dose_x_resolution, dose_y_resolution, dose_z_resolution, context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation resolution", "mm", dose_x_resolution.ToString() + "x" + dose_y_resolution.ToString() + "x" + dose_z_resolution.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"grid_size", 3, hospital);
                }

                //Dose to water eller medium
                //Mål: Dose to medium for alle bortset fra HH
                string dose_media;
                if (algorithm.Contains("acuros"))
                {

                    if (plan_name.Contains("hh"))
                    {
                        dose_media = "water";
                    }
                    else
                    {
                        dose_media = "medium";
                    }

                    try
                    {
                        string dose_rep;
                        context.PlanSetup.PhotonCalculationOptions.TryGetValue("DoseReportingMode", out dose_rep);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation medium", "", "Dose to " + dose_media, dose_rep, trafik_lys, plancheck_test.Plancheck_functions.dose_media(dose_media, dose_rep));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "dose_medium", plancheck_test.Plancheck_functions.dose_media(dose_media, dose_rep), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose calculation medium", "", "Dose to " + dose_media, e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "dose_medium", 3, hospital);
                    }
                }

                //Normering
                //Mål: fra database
                string target_normalization = null;
                try
                {
                    switch (norm_volume)
                    {
                        case "dmean":
                            target_normalization = norm_procent.ToString() + "% at Target Mean. Target: " + norm_target.ToUpper();
                            break;
                        case "dmin":
                            target_normalization = norm_procent.ToString() + "% at Target Minimum. Target: " + norm_target.ToUpper();
                            break;
                        default:
                            if (norm_volume != null && Regex.IsMatch(norm_volume, @"^\d+$"))
                            {
                                target_normalization = norm_procent.ToString() + ".00% covers " + norm_volume + ".00% of Target Volume. Target: " + norm_target.ToUpper();
                            }
                            break;
                    }

                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Normalization method", "", "", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"Normalization", 3, hospital);
                }

                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Normalization method", "", target_normalization, context.PlanSetup.PlanNormalizationMethod + ". Target: " + context.PlanSetup.TargetVolumeID, trafik_lys, plancheck_test.Plancheck_functions.normalization(norm_procent, norm_target, norm_volume, context));
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"Normalization", plancheck_test.Plancheck_functions.normalization(norm_procent, norm_target, norm_volume, context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Normalization method", "", target_normalization, e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"Normalization", 3, hospital);
                }

                
                //Dmean til target
                //Mål: 100 plus/minus 1% til alle undtagen stereotaksier
                double dmean_target = 1;
                if (plan_name.Substring(0, 2).Contains("sr") || plan_name.Substring(0, 2).Contains("hp") || plan_name.Substring(0, 2).Contains("tb"))
                {
                    //Do nothing
                }
                else
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dmean to target", "%", "100±" + dmean_target.ToString(), Math.Round(context.PlanSetup.GetDVHCumulativeData(context.StructureSet.Structures.Where(n => n.Id.Equals(context.PlanSetup.TargetVolumeID)).First(), DoseValuePresentation.Relative, VolumePresentation.Relative, 0.1).MeanDose.Dose, 1).ToString(), trafik_lys, plancheck_test.Plancheck_functions.dmean(dmean_target, context));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"Dmean_target", plancheck_test.Plancheck_functions.dmean(dmean_target, context), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dmean to target", "%", "100±" + dmean_target.ToString(), e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"Dmean_target", 3, hospital);
                    }
                }


                //Fysisk referencepunkt
                //Mål: Virtuelt reference punkt
                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point", "", "Virtual reference point", context.PlanSetup.Beams.First(n => n.IsSetupField == false).FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id).ReferencePoint.Id, trafik_lys, plancheck_test.Plancheck_functions.virtual_refpoint(context));
                get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"virtual_referencepoint", plancheck_test.Plancheck_functions.virtual_refpoint(context), hospital);

                //Fysisk referencepunkt ligger i target
                //Mål: Ref punkt skal altid ligge i target strukturen

                if (!double.IsNaN(context.PlanSetup.Beams.First(n => n.IsSetupField == false).FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id).RefPointLocation.x))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point", "", "Physical reference point in target", context.PlanSetup.Beams.First().FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id).Id, trafik_lys, plancheck_test.Plancheck_functions.refpoint_target(context));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"physical_referencepoint", plancheck_test.Plancheck_functions.refpoint_target(context), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point", "", "Physical reference point in target", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"physical_referencepoint", 3, hospital);
                    }
                }


                // Dosis til ref punkt
                //Mål: 100%
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose to reference point", "%", "100", (context.PlanSetup.PlannedDosePerFraction / context.PlanSetup.DosePerFraction.Dose * 100).ToString(), trafik_lys, plancheck_test.Plancheck_functions.dose_2_refpoint(context));
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"dose_referencepoint", plancheck_test.Plancheck_functions.dose_2_refpoint(context), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dose to reference point", "%", "100", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"dose_referencepoint", 3, hospital);
                }

                //Refpunkt dosisgrænser
                //Mål: Dosis passer med fraktionering
                double session_limit = fx_dosis;
                double daily_limit = fx_dosis;
                if (plan_name == "lu007" || plan_name == "lu009" || plan_name == "tb011" || plan_name == "tb017")
                {
                    daily_limit = 2 * daily_limit;
                }
                double total_limit = fx_dosis * fx;
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point session dose limit", "Gy", Math.Round(session_limit, 3).ToString(), context.ExternalPlanSetup.PrimaryReferencePoint.SessionDoseLimit.Dose.ToString(), trafik_lys, plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.SessionDoseLimit.Dose, session_limit));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "session_referencepoint", plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.SessionDoseLimit.Dose, session_limit), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point session dose limit", "Gy", "", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "session_referencepoint", 3, hospital);
                }
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point daily dose limit", "Gy", Math.Round(daily_limit, 3).ToString(), context.ExternalPlanSetup.PrimaryReferencePoint.DailyDoseLimit.Dose.ToString(), trafik_lys, plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.DailyDoseLimit.Dose, daily_limit));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "daily_referencepoint", plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.DailyDoseLimit.Dose, daily_limit), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point daily dose limit", "Gy", "", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "daily_referencepoint", 3, hospital);
                }
                try
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point total dose limit", "Gy", Math.Round(total_limit, 3).ToString(), context.ExternalPlanSetup.PrimaryReferencePoint.TotalDoseLimit.Dose.ToString(), trafik_lys, plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.TotalDoseLimit.Dose, total_limit));
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "total_referencepoint", plancheck_test.Plancheck_functions.refpoint_limit(context.ExternalPlanSetup.PrimaryReferencePoint.TotalDoseLimit.Dose, total_limit), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Reference point total dose limit", "Gy", "", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "total_referencepoint", 3, hospital);
                }


                // MU/Gy
                //Mål: <300 for RA, mellem 80 og 120 for 3DCRT, mellem 180 og 220 for CM med glandler, kiler skaleres med kilefaktor, ingen grænse for Halcyon
                //For TBI: 150 MU/Gy korrigeret for afstandskvadrat lov = 135*3.5^2, usikkerhed +-100

                double dynamic_mu_limit = 300;
                double crt_mu_limit = 100;
                double crt_tolerance_mu = 20;
                double tbi_mu_limit = 1650;
                double tbi_tolerance_mu = 200;
                double halcyon_mu_tolerance = 500;
                bool is_crt = false;
                double total_mu = context.PlanSetup.Beams.Where(n => n.IsSetupField == false).Sum(n => n.Meterset.Value);
                double dynamic_mu = context.PlanSetup.Beams.Where(n => n.IsSetupField == false && (n.Technique.Id.ToLower().Contains("arc") || n.MLCPlanType.ToString().ToLower() == "dose dynamic")).Sum(n => n.Meterset.Value);
                double crt_mu = context.PlanSetup.Beams.Where(n => n.IsSetupField == false && n.Technique.Id.ToLower().Contains("static") && n.MLCPlanType.ToString().ToLower() != "dose dynamic").Sum(n => n.Meterset.Value);


                double temp_mu_limit = crt_mu_limit;
                try
                {
                    foreach (var beam in context.PlanSetup.Beams.Where(n => n.Wedges.Count() > 0))
                    {
                        crt_mu_limit = crt_mu_limit + temp_mu_limit * (beam.Meterset.Value / total_mu / plancheck_test.Plancheck_functions.wedge_factor(Convert.ToInt16(beam.EnergyModeDisplayName.Remove(beam.EnergyModeDisplayName.Length - 1)), Convert.ToInt16(beam.Wedges.First().WedgeAngle)) - beam.Meterset.Value / total_mu);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MU/Gy", "", "Wedge factor", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"MU_Gy", 3, hospital);
                }

                double mu_limit;

                if (is_tbi)
                {
                    mu_limit = tbi_mu_limit;
                    crt_tolerance_mu = tbi_tolerance_mu;
                    is_crt = true;
                }
                else if (dynamic_mu == total_mu)
                {
                    mu_limit = dynamic_mu_limit;
                }
                else if (crt_mu == total_mu)
                {
                    if (plan_name.Contains("cm") && !(new[] { "cm051", "cm052", "cm053", "cm060", "cm061", "cm062", "cm063" }.Contains(plan_name.Substring(0, 5))))
                    {
                        mu_limit = 2 * crt_mu_limit;
                        is_crt = true;
                    }
                    else
                    {
                        mu_limit = crt_mu_limit;
                        is_crt = true;
                    }
                }
                else
                {
                    mu_limit = (dynamic_mu_limit * dynamic_mu / total_mu + crt_mu_limit * crt_mu / total_mu);
                }
                if (halcyon.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    mu_limit = halcyon_mu_tolerance;
                }


                try
                {
                    if (is_crt)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MU/Gy", "MU/Gy", Math.Round(mu_limit).ToString() + "±" + crt_tolerance_mu.ToString(), Math.Round(total_mu / context.PlanSetup.DosePerFraction.Dose).ToString(), trafik_lys, plancheck_test.Plancheck_functions.mu_gy(total_mu / context.PlanSetup.DosePerFraction.Dose, mu_limit, crt_tolerance_mu));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"MU_Gy", plancheck_test.Plancheck_functions.mu_gy(total_mu / context.PlanSetup.DosePerFraction.Dose, mu_limit, crt_tolerance_mu), hospital);
                    }
                    else
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MU/Gy", "MU/Gy", Math.Round(mu_limit).ToString(), Math.Round(total_mu / context.PlanSetup.DosePerFraction.Dose).ToString(), trafik_lys, plancheck_test.Plancheck_functions.mu_gy(total_mu / context.PlanSetup.DosePerFraction.Dose, mu_limit));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"MU_Gy", plancheck_test.Plancheck_functions.mu_gy(total_mu / context.PlanSetup.DosePerFraction.Dose, mu_limit), hospital);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "MU/Gy", "MU/Gy", mu_limit.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"MU_Gy", 3, hospital);
                }


                //Max MU per field
                //Mål: TBI 800 MU
                if (is_tbi)
                {
                    double field_mu_limit = 800;

                    try
                    {
                        bool test = true;
                        foreach (var beam in context.PlanSetup.Beams)
                        {
                            if (plancheck_test.Plancheck_functions.tbi_max_mu_per_field(beam, field_mu_limit) == 3)
                            {
                                test = false;
                            }
                        }
                        if (test)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per field", "MU", field_mu_limit.ToString(), $"All field have less than {field_mu_limit} MU", trafik_lys, 1);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_max_mu", 1, hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per field", "MU", field_mu_limit.ToString(), $"{context.PlanSetup.Beams.First(n => n.Meterset.Value > field_mu_limit).Id} has {context.PlanSetup.Beams.First(n => n.Meterset.Value > field_mu_limit).Meterset.Value} MU", trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_max_mu", 3, hospital);
                        }
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per field", "MU", field_mu_limit.ToString(), e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_max_mu", 3, hospital);
                    }
                }

                //Max 2500 MU per beam for ET dynamic
                double max_mu_ETD = 2500;
                string[] ET_dynamic = { "Accelerator02","Accelerator03" };
                if (ET_dynamic.Contains(context.PlanSetup.Beams.First().TreatmentUnit.Id))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per beam for ET Dynamic", "MU", max_mu_ETD.ToString(), $"All field have less than {max_mu_ETD} MU", trafik_lys, plancheck_test.Plancheck_functions.max_mu(max_mu_ETD, context.PlanSetup.Beams));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "max_mu_ETD", plancheck_test.Plancheck_functions.max_mu(max_mu_ETD, context.PlanSetup.Beams), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per beam for ET Dynamic", "MU", max_mu_ETD.ToString(), $"All field have less than {max_mu_ETD} MU", trafik_lys, plancheck_test.Plancheck_functions.max_mu(max_mu_ETD, context.PlanSetup.Beams));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "max_mu_ETD", 3, hospital);
                    }
                }

                //Max 999 MU per beam for SRK med static MLC
                double max_mu_static = 999;
                if (context.PlanSetup.Beams.Any(n => n.IsSetupField == false && (n.Technique.Id.ToLower().Contains("arc") & n.MLCPlanType.ToString().ToLower() == "static")))
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per beam for Static MLC", "MU", max_mu_static.ToString(), $"All field have less than {max_mu_static} MU", trafik_lys, plancheck_test.Plancheck_functions.max_mu(max_mu_static, context.PlanSetup.Beams.Where(n => n.IsSetupField == false && (n.Technique.Id.ToLower().Contains("arc") & n.MLCPlanType.ToString().ToLower() == "static"))));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "max_mu_static", plancheck_test.Plancheck_functions.max_mu(max_mu_static, context.PlanSetup.Beams), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Max MU per beam for Static MLC", "MU", max_mu_static.ToString(), $"All field have less than {max_mu_static} MU", trafik_lys, plancheck_test.Plancheck_functions.max_mu(max_mu_static, context.PlanSetup.Beams.Where(n => n.IsSetupField == false && (n.Technique.Id.ToLower().Contains("arc") & n.MLCPlanType.ToString().ToLower() == "static"))));
                        get_diagnose_from_DB.output_summary(anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "max_mu_static", 3, hospital);
                    }
                }

                //Doserate for TBI
                //Mål: TBI 100 MU/min
                if (is_tbi)
                {
                    double field_doserate_limit = 100;

                    try
                    {
                        bool test = true;
                        foreach (var beam in context.PlanSetup.Beams)
                        {
                            if (plancheck_test.Plancheck_functions.tbi_doserate(beam.DoseRate, field_doserate_limit) == 3)
                            {
                                test = false;
                            }

                        }
                        if (test)
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Doserate", "MU/min", field_doserate_limit.ToString(), $"All fields have {field_doserate_limit} doserate", trafik_lys, 1);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_doserate", 1, hospital);
                        }
                        else
                        {
                            VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Doserate", "MU/min", field_doserate_limit.ToString(), $"{context.PlanSetup.Beams.First(n => n.DoseRate != field_doserate_limit).Id} has doserate {context.PlanSetup.Beams.First(n => n.DoseRate != field_doserate_limit).DoseRate}", trafik_lys, 3);
                            get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_doserate", 3, hospital);
                        }
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Doserate", "MU/min", field_doserate_limit.ToString(), e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "tbi_doserate", 3, hospital);
                    }
                }
                //Treat time
                //Mål: TBI 9 min

                double time_limit = 9;

                try
                {
                    bool test = true;
                    foreach (var beam in context.PlanSetup.Beams)
                    {
                        if (plancheck_test.Plancheck_functions.treatment_time(beam.TreatmentTime/60, time_limit) == 3)
                        {
                            test = false;
                        }

                    }
                    if (test)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment time", "min", time_limit.ToString(), $"All fields have treatment times <{time_limit}", trafik_lys, 1);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "treatment_time", 1, hospital);
                    }
                    else
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment time", "min", time_limit.ToString(), $"{context.PlanSetup.Beams.First(n => n.TreatmentTime/60 >time_limit).Id} has treatment time {context.PlanSetup.Beams.First(n => n.TreatmentTime/60 > time_limit).TreatmentTime/60}", trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "treatment_time", 3, hospital);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Treatment time", "min", time_limit.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "treatment_time", 3, hospital);
                }

                //Check af normalization value
                //Mål: For modulerede planer: Tentativt 95-110
                double norm_low = 95;
                double norm_high = 110;
                try
                {
                    if (context.PlanSetup.Beams.Where(n => n.IsSetupField == false && (n.MLCPlanType.ToString().ToLower().Contains("vmat") || n.MLCPlanType.ToString().ToLower() == "dose dynamic")).Count() > 0)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Plan normalization value", "", "Between " + norm_low.ToString() + " and " + norm_high.ToString(), Math.Round(context.PlanSetup.PlanNormalizationValue).ToString(), trafik_lys, plancheck_test.Plancheck_functions.plan_norm_value(context, norm_low, norm_high));
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"norm_value", plancheck_test.Plancheck_functions.plan_norm_value(context, norm_low, norm_high), hospital);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Plan normalization value", "", "Between " + norm_low.ToString() + " and " + norm_high.ToString(), e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"norm_value", 3, hospital);
                }


                //Check er der er lower på risiko organer (alle organer som ikke er CTV, PTV eller GTV)???
                //Mål: Kun strukturer som er type PTV, CTV, GTV og hjælpestrukturer må have lower constraints.
                try
                {
                    string f_structure = "";
                    plancheck_test.Plancheck_functions.lower_objective_oar(context, out f_structure);
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Lower objective on OAR", "", "None", f_structure, trafik_lys, plancheck_test.Plancheck_functions.lower_objective_oar(context, out f_structure));
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"lower_OAR", plancheck_test.Plancheck_functions.lower_objective_oar(context, out f_structure), hospital);
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Lower objective on OAR", "", "None", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"lower_OAR", 3, hospital);
                }

                // Dmax i target
                //Mål: Dmax i target (vil ofte ikke være overholdt på konventionelle felter, specielt mamma
                if (!is_tbi)
                {
                    try
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dmax in target", "", "True", context.StructureSet.Structures.First(n => n.Id.Equals(context.PlanSetup.TargetVolumeID)).IsPointInsideSegment(context.PlanSetup.Dose.DoseMax3DLocation).ToString(), trafik_lys, plancheck_test.Plancheck_functions.dmax_in_target(context));
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "Dmax_target", plancheck_test.Plancheck_functions.dmax_in_target(context), hospital);
                    }
                    catch (Exception e)
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Dmax in target", "", "True", e.ToString(), trafik_lys, 3);
                        get_diagnose_from_DB.output_summary( anon_ID, context.Course.Id, context.PlanSetup.Id, context.CurrentUser.Id, "Dmax_target", 3, hospital);
                    }
                }

                //Pres på normal væv for optimering
                //Mål: der er brugt NTO eller ring med upper constraint
                try
                {
                    if (context.PlanSetup.OptimizationSetup.Objectives.Count() > 1)
                    {
                        string norm_press;
                        int eval = plancheck_test.Plancheck_functions.normal_tissue_objective(context, out norm_press);
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "NTO or Ring for dynamic plans", "", "True",norm_press , trafik_lys,eval );
                        get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"normal_objective", eval, hospital);
                    }
                }
                catch (Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "NTO or Ring for dynamic plans", "", "True", e.ToString(), trafik_lys, 3);
                    get_diagnose_from_DB.output_summary(anon_ID,context.Course.Id,context.PlanSetup.Id,context.CurrentUser.Id,"normal_objective", 3, hospital);
                }

                try
                {
                    if (plan_name.ToLower().Contains("hh020"))
                    {
                        VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Write matchinstruks for HH020 in document", "", "OBS", "OBS", trafik_lys, 6);
                    }
                    
                }
                catch(Exception e)
                {
                    VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Write matchinstruks for HH020 in document", "", "OBS", "OBS", trafik_lys, 3);
                }





            }
            catch (Exception e)
            {
                VMS.TPS.PQMReporter.WriteGeneralStatisticsXML(patient, ss, plan, writer, "Major error", "", "", e.ToString(), "r", 3);
            }




        }


        public static void manual(ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer, string hospital)
        {
            try
            {


                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Book-keeping", "Stemmer plan ID med ordination i patientdokument", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er planen lavet til den korrekte apparat type i forhold til hvor patientens fraktioner er booket", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "CT-scan", "Patientmarkører: Stemmer user origin med patientmarkører (for DIBH forventes mindre uoverensstemmelse)?", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er der pacemaker eller ICD og er der taget højde for disse i henhold til retningslinje", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er der metal artefakter eller andre artefakter? er der brugt IMAR korrekt?", "r");
                if ((context.PlanSetup.Id.ToLower().Contains("lu") || context.PlanSetup.Id.ToLower().Contains("hp")) && (context.Image.Comment.ToLower().Contains("breathhold") || context.Image.Comment.ToLower().Contains("dibh") || context.Image.Comment.Contains("Resp 2.0 B30f")))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er valgt af behandlingsscan og marginer checket", "r"); //Kun ved 4D eller flere DIBH
                }
                if (context.PlanSetup.Id.ToLower().Contains("cm"))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er der brugt DIBH korrekt", "r"); //kun CM
                }
                if (context.Image.Comment.ToLower().Contains("breathhold") || context.Image.Comment.ToLower().Contains("dibh"))
                {
                    //VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "'Use gated' skal være klikket til i planen. Importer gating kurven til documents", "r"); //DIBH
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Check at gatingkurven ligger i den korrekte database og ser rigtig ud", "r"); //DIBH
                }
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Contouring", "Er target indtegnet så det stemmer med hvad der er ordineret i dokumentet", "r");
                if (!(context.PlanSetup.Id.ToLower().Contains("hh") || context.PlanSetup.Id.ToLower().Contains("cn") || context.PlanSetup.Id.ToLower().Contains("srk") || context.PlanSetup.Id.ToLower().Contains("srt") || context.PlanSetup.Id.ToLower().Contains("lu008")))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Lejet skal være placeret så øverste kant sammenfalder med øverste kant på CT leje", "r"); //Ikke HH, CN, SRK, SRT
                }
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Hele kroppen skal være indkluderet i body, også fysisk bolus", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "PTV og CTV skal være trukket ind fra body med mindre hud medbehandles", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er indtegninger fornuftige (sammenhængende og uden overlap)", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Passer PTV margin med retningslinje eller dokument", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er der anvendt hjælpestruktur til at overskrive artefakter, midlertidig luft i PTV eller kontrast i årer (>200 HU). Check at der er brugt vand", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Bolus", "Burde der være bolus, er det nævnt i dokumentet", "r");
                if (context.StructureSet.Structures.Where(n => n.DicomType.ToLower().Contains("bolus")).Count() > 0)
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er bolus placeret optimalt, er der korrekt tykkelse, er der valgt vand", "r"); //Kun ved virtuel bolus
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Print beams eye view til at hjælpe med placering, skriv hvor står bolus er og hvordan den skal placeres i dokumentet", "r"); //Kun ved virtuel bolus
                }
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Fields", "Er behandlingsteknik og felt konfiguration optimal til behandlingen, er der taget højde for andre og evt. tidligere targets", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Er isocenter placeret hensigtsmæssigt, ca i midten af target, evt. mellem targets", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Kontroller at kæber og MLC er fornuftige", "r");
                if (context.PlanSetup.Id.ToLower().Contains("cm"))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Ligger sammenstykningen fornuftigt, felter som krydser sammenstykningen må ikke have for mange MU (~15)", "r"); //Kun CM
                }
                if (context.PlanSetup.Beams.Where(n => n.Technique.ToString().ToLower().Contains("arc")).Count() > 0)
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "RapidArc", "Er arm indtegnet og presset på, alternativt er der brugt avoidance", "r"); //Kun RA
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Giver antal arcs brugt mening", "r"); //Kun RA
                   
                }

                if (context.PlanSetup.Id.ToLower().Contains("cm060"))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Setup", "Check at der er et MV setupfelt som viser clips og thoraxbue", "r"); //kun CM060
                }
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Skal der matches anderledes end normalt, så dokumenter i ark hvad der skal ske", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Dosecalculation", "Den røde boks skal dække hele det relevante område, typisk hele patienten. Dog for elektroner kun PTV plus 5 cm", "r");



                if (!double.IsNaN(context.PlanSetup.Beams.First(n => n.IsSetupField == false).FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id).RefPointLocation.x))
                {
                    VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Ligger referencepunkt fornuftigt (i midten af target, ikke på en gradient)", "r");//Kun fysisk ref punkt
                }

                //VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Dosisgrænser under referencepunkt passer med fraktionering", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Dose", "Visuel inspektion af dosis, er der dækning, er der presset fornuftigt, ligger der unødvendig højdosis uden for target", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Vurder om der er overlap med evt. tidligere behandlinger", "r");
                VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "", "Vurder dosismax placering og værdi", "r");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }

        }


        //public static void Manual_dose(ScriptContext context, Patient patient, StructureSet ss, PlanSetup plan, XmlWriter writer)
        //{
        //    try
        //    {


        //        string plan_name;
        //        if (context.PlanSetup.Id.ToLower().Substring(0, 2) == "cm")
        //        {
        //            if (context.PlanSetup.Beams.First().IsocenterPosition.x < 0)
        //            {
        //                plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "h";
        //            }
        //            else
        //            {
        //                plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5) + "v";
        //            }

        //        }
        //        else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srk") //Der er 6 tegn i SRK koder, derfor skal de håndteres lidt anderledes
        //        {
        //            plan_name = context.PlanSetup.Id.ToLower().Substring(0, 6);
        //        }
        //        else if (context.PlanSetup.Id.Substring(0, 3).ToLower() == "srt")
        //        {
        //            plan_name = context.PlanSetup.Id.ToLower().Substring(0, 3);
        //        }
        //        else if (context.PlanSetup.Id.Length > 4)// resten af koderne har kun 5
        //        {
        //            plan_name = context.PlanSetup.Id.ToLower().Substring(0, 5);
        //        }
        //        else
        //        {
        //            plan_name = context.PlanSetup.Id.ToLower() + "nnnnnnn";
        //        }

        //        if (plan_name.Substring(0, 2) != "cm" && plan_name.Substring(plan_name.Length - 3) == "000")// HVIS 000 kode så laver den en XX000_d hvor d er fraktionsdosis så der kan oprettes i diagnose tabel og prioritet et sæt for den situation
        //        {
        //            plan_name = plan_name + "_" + context.PlanSetup.UniqueFractionation.PrescribedDosePerFraction.Dose.ToString();
        //        }
        //        else if (plan_name.Substring(0, 2) == "cm" && plan_name.Substring(plan_name.Length - 4, 3) == "000")
        //        {
        //            plan_name = plan_name + "_" + context.PlanSetup.UniqueFractionation.PrescribedDosePerFraction.Dose.ToString();
        //        }

                
        //        DB_parser_copy get_diagnose_from_DB = new DB_parser_copy();

        //        bool DB_status = get_diagnose_from_DB.Read_diagnose(plan_name);

        //        int fx = 0;
        //        int diag_index = 0;
        //        double fx_dosis = 2;
        //        double norm_procent = 100;
        //        string norm_target = "ptv";
        //        string norm_volume = "dmean";
        //        string setup = null;
                
        //            diag_index = DB_parser_copy.DB_diag.DB_diag_code;
        //            fx = DB_parser_copy.DB_diag.DB_fraktioner;
        //            fx_dosis = DB_parser_copy.DB_diag.DB_fx_dosis;
        //            norm_procent = DB_parser_copy.DB_diag.DB_norm_procent;
        //            norm_target = DB_parser_copy.DB_diag.DB_norm_target;
        //            norm_volume = DB_parser_copy.DB_diag.DB_norm_volume;
        //            setup = DB_parser_copy.DB_diag.DB_setup;
                

        //        bool DB_structures_status = get_diagnose_from_DB.Read_priority(diag_index);

        //        if (DB_status && !DB_structures_status)
        //        {


        //            // DEr skal være noget fejl meddelse og noget default opførsel når der ikke er constraints
        //        }

        //        int[] prioritet = new int[0];
        //        if (DB_structures_status)
        //        {

        //            prioritet = DB_parser_copy.DB_structures.DB_strukt;

        //            int counter = 0;
        //            Constraint constraints = new Constraint();

        //            if (prioritet != null)
        //            {
        //                constraints.type = new string[prioritet.Length];
        //                constraints.navn = new string[prioritet.Length];
        //                constraints.req = new bool[prioritet.Length];
        //                constraints.dmean = new double[prioritet.Length];
        //                constraints.rel_vol_dose = new double[prioritet.Length];
        //                constraints.rel_vol = new double[prioritet.Length];
        //                constraints.abs_vol_dose = new double[prioritet.Length];
        //                constraints.abs_vol = new double[prioritet.Length];
        //                constraints.mere_end_mindre_end = new int[prioritet.Length];
        //                constraints.trafik_lys = new string[prioritet.Length];
        //                constraints.kommentar = new string[prioritet.Length];

        //                while (counter < prioritet.Length)
        //                {


        //                    if (get_diagnose_from_DB.Read_structure(prioritet[counter]))
        //                    {


        //                        constraints.type[counter] = DB_parser_copy.DB_constraints.type;
        //                        constraints.navn[counter] = DB_parser_copy.DB_constraints.DB_navn;
        //                        constraints.req[counter] = DB_parser_copy.DB_constraints.DB_req;
        //                        constraints.dmean[counter] = DB_parser_copy.DB_constraints.DB_dmean;
        //                        constraints.rel_vol_dose[counter] = DB_parser_copy.DB_constraints.DB_rel_vol_dose;
        //                        constraints.rel_vol[counter] = DB_parser_copy.DB_constraints.DB_rel_vol;
        //                        constraints.abs_vol_dose[counter] = DB_parser_copy.DB_constraints.DB_abs_vol_dose;
        //                        constraints.abs_vol[counter] = DB_parser_copy.DB_constraints.DB_abs_vol;
        //                        constraints.mere_end_mindre_end[counter] = DB_parser_copy.DB_constraints.DB_mere_end_mindre_end;
        //                        constraints.trafik_lys[counter] = DB_parser_copy.DB_constraints.DB_trafik_lys;
        //                        constraints.kommentar[counter] = DB_parser_copy.DB_constraints.DB_kommentar;

        //                        DB_parser_copy.DB_constraints.type = null;
        //                        DB_parser_copy.DB_constraints.DB_navn = null;
        //                        DB_parser_copy.DB_constraints.DB_req = false;
        //                        DB_parser_copy.DB_constraints.DB_dmean = double.NaN;
        //                        DB_parser_copy.DB_constraints.DB_rel_vol_dose = double.NaN;
        //                        DB_parser_copy.DB_constraints.DB_rel_vol = double.NaN;
        //                        DB_parser_copy.DB_constraints.DB_abs_vol_dose = double.NaN;
        //                        DB_parser_copy.DB_constraints.DB_abs_vol = double.NaN;
        //                        DB_parser_copy.DB_constraints.DB_mere_end_mindre_end = 0;
        //                        DB_parser_copy.DB_constraints.DB_trafik_lys = null;
        //                        DB_parser_copy.DB_constraints.DB_kommentar = null;
        //                    }

        //                    counter++;
        //                }

        //                for (int n = 0; n < prioritet.Count(); n++)
        //                {


        //                    switch (constraints.type[n])
        //                    {
        //                        case "manual":
        //                            VMS.TPS.PQMReporter.WriteManualStatisticsXML(patient, ss, plan, writer, "Manual dose check", constraints.navn[n] + ": " + constraints.kommentar[n], "r");
                                   
        //                            continue;
        //                        default:
        //                            continue;


        //                    }

        //                }

        //            }


        //        }


        //    }
        //    catch (Exception e)
        //    {
        //        System.Windows.MessageBox.Show(e.ToString());
        //    }
            
        //}

        public static void Cleanup()
        {
            DB_parser_copy.DB_constraints.DB_abs_vol = 0;
            DB_parser_copy.DB_constraints.DB_abs_vol_dose = 0;
            DB_parser_copy.DB_constraints.DB_rel_vol = 0;
            DB_parser_copy.DB_constraints.DB_rel_vol_dose = 0;
            DB_parser_copy.DB_constraints.DB_dmean = 0;
            DB_parser_copy.DB_constraints.DB_kommentar = null;
            DB_parser_copy.DB_constraints.DB_mere_end_mindre_end = 0;
            DB_parser_copy.DB_constraints.DB_navn = null;
            DB_parser_copy.DB_constraints.DB_req = false;
            DB_parser_copy.DB_constraints.DB_trafik_lys = null;
            DB_parser_copy.DB_constraints.type = null;

            DB_parser_copy.DB_diag.DB_diag_code = 0;
            DB_parser_copy.DB_diag.DB_fraktioner = 0;
            DB_parser_copy.DB_diag.DB_fx_dosis = 0;
            DB_parser_copy.DB_diag.DB_norm_procent = 0;
            DB_parser_copy.DB_diag.DB_norm_target = null;
            DB_parser_copy.DB_diag.DB_norm_volume = null;
            DB_parser_copy.DB_diag.DB_setup = null;
            DB_parser_copy.DB_diag.DB_DIBH = 0;

            DB_parser_copy.DB_structures.DB_strukt = null;
            
        }
    }
}

