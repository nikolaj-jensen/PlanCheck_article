using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Text.RegularExpressions;
using System.Windows;

namespace plancheck_test
{
    
    class Plancheck_functions
    {
        
        public static int number_of_fractions(int num_fx, ScriptContext context)
        {
            
            int evaluation = 1;
            if (num_fx != context.PlanSetup.NumberOfFractions)
            {
                evaluation = 3;
                
            }

            return evaluation;

        }

        public static int dose_per_fraction(double fx_dosis, ScriptContext context)
        {
            int evaluation = 1;
            if (fx_dosis != context.PlanSetup.DosePerFraction.Dose)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int scan_name(string target, ScriptContext context)
        {
            int evaluation = 1;
            if (!context.Image.Id.ToLower().Contains(target.ToLower()))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int scan_date(DateTime target, ScriptContext context)
        {
            int evaluation = 1;
            if (DateTime.Compare(context.Image.CreationDateTime.GetValueOrDefault(new DateTime(1980, 12, 16)), target) <= 0)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int couch_type(string target, ScriptContext context)
        {
            int evaluation = 1;

            if (!context.PlanSetup.StructureSet.Structures.Where(n => n.Id.Contains("CouchInterior")).FirstOrDefault().Name.ToLower().Contains(target.ToLower()))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int couch_HU(int target, int HU)
        {
            int evaluation = 1;

            if (target != HU)
            {
                evaluation = 3;
            }
            else
            {
                evaluation = 1;
            }
            return evaluation;
        }

        public static int course_diagnose(string target, ScriptContext context)
        {
            int evaluation = 1;

            if (context.Course.Diagnoses.Count(n => n.Code.ToLower().Contains(target.ToLower())) == 0)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int virtual_bolus(int target, Beam beam)
        {
            int evaluation = 1;

            if (beam.Boluses.Count() != target)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int same_isocenter(double target_x, double target_y, double target_z, ScriptContext context)
        {
            int evaluation = 1;

            if (context.PlanSetup.Beams.Where(n => n.IsocenterPosition.x == target_x).Count() != context.PlanSetup.Beams.Count() || context.PlanSetup.Beams.Where(n => n.IsocenterPosition.y == target_y).Count() != context.PlanSetup.Beams.Count() || context.PlanSetup.Beams.Where(n => n.IsocenterPosition.z == target_z).Count() != context.PlanSetup.Beams.Count())
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int setup_name(Beam beam)
        {
            int evaluation = 1;

            if (!beam.Id.ToLower().Contains(beam.ControlPoints.First().GantryAngle.ToString()))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int treat_name(Beam beam, int target, bool is_tbi)
        {
            int evaluation = 1;
            if (!is_tbi)
            {
                if (target == 1)
                {
                    if (!Regex.IsMatch(beam.Id.Substring(0, 1), @"^\d+$"))
                    {
                        evaluation = 3;
                    }
                }
                else
                {
                    if (!Regex.IsMatch(beam.Id.Substring(0, 2), @"^\d+$"))
                    {
                        evaluation = 3;
                    }
                }
            }
            else
            {
                if (!beam.Id.ToLower().Contains("setup"))
                {
                    if (!Regex.IsMatch(beam.Id.Substring(6, 2), @"^\d+$"))
                    {
                        evaluation = 3;
                    }
                }
            }

            return evaluation;
        }

        public static int DIBH_wedge(Beam beam, ScriptContext context)
        {
            int evaluation = 1;
            if (context.Image.Comment.ToLower().Contains("dibh") || context.Image.Comment.ToLower().Contains("breathhold"))
            {
                if (beam.Wedges.Any(m => m.Id.ToLower().Contains("edw")))
                {
                    evaluation = 3;
                }
            }
            return evaluation;
        }

        public static int arc_cardinal(Beam beam)
        {
            int evaluation = 1;

            if (beam.ControlPoints.First().CollimatorAngle % 90 == 0)
            {

                evaluation = 3;

            }

            return evaluation;
        }

        public static int arc_overlap(Beam beam, ScriptContext context)
        {
            int evaluation = 1;

            if (context.PlanSetup.Beams.Where(n => n.ControlPoints.First().CollimatorAngle.Equals(beam.ControlPoints.First().CollimatorAngle)).Where(n => n.ControlPoints.First().PatientSupportAngle.Equals(beam.ControlPoints.First().PatientSupportAngle)).Count() > 1)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int arc_x_coll(Beam beam, double target)
        {
            int evaluation = 1;

            if ((-beam.ControlPoints.First().JawPositions.X1 / 10 + beam.ControlPoints.First().JawPositions.X2 / 10) > target)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int y_coll(Beam beam, double target)
        {
            int evaluation = 1;

            if (beam.ControlPoints.First().JawPositions.Y1 / 10 < -target || beam.ControlPoints.First().JawPositions.Y2 / 10 > target)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int setup_coll(Beam beam, double target)
        {
            int evaluation = 1;

            if ((-beam.ControlPoints.First().JawPositions.X1 / 10 + beam.ControlPoints.First().JawPositions.X2 / 10) != target || (-beam.ControlPoints.First().JawPositions.Y1 / 10 + beam.ControlPoints.First().JawPositions.Y2 / 10) != target)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int cbct_bones(ScriptContext context)
        {
            int evaluation = 1;

            if (!context.StructureSet.Structures.Any(n => (n.Id.Equals("bone", StringComparison.OrdinalIgnoreCase) || n.Id.Equals("bones", StringComparison.OrdinalIgnoreCase)) && !n.IsEmpty))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int dose_algorithm(string target, ScriptContext context)
        {
            int evaluation = 1;

            if (!context.PlanSetup.PhotonCalculationModel.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int dose_resolution(double target_x, double target_y, double target_z, ScriptContext context)
        {
            int evaluation = 1;

            if (context.PlanSetup.Dose.XRes > target_x || context.PlanSetup.Dose.YRes > target_y || context.PlanSetup.Dose.ZRes > target_z)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int dose_media(string target, string value)
        {
            int evaluation = 1;

            if (!value.Contains(target))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int dmean(double target, ScriptContext context)
        {
            int evaluation = 1;

            if (Math.Abs(Math.Round(context.PlanSetup.GetDVHCumulativeData(context.StructureSet.Structures.Where(n => n.Id.Equals(context.PlanSetup.TargetVolumeID)).First(), DoseValuePresentation.Relative, VolumePresentation.Relative, 0.1).MeanDose.Dose, 1) - 100) > target)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int virtual_refpoint(ScriptContext context)
        {
            int evaluation = 1;
            var fieldRefPoint = context.PlanSetup.Beams.First(n => n.IsSetupField == false).FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id);
            if (!double.IsNaN(fieldRefPoint.RefPointLocation.x))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int refpoint_target(ScriptContext context)
        {
            int evaluation = 1;
            var fieldRefPoint = context.PlanSetup.Beams.First().FieldReferencePoints.First(frp => frp.ReferencePoint.Id == context.PlanSetup.PrimaryReferencePoint.Id);
            if (!context.StructureSet.Structures.First(n => n.Id.Equals(context.PlanSetup.TargetVolumeID)).IsPointInsideSegment(fieldRefPoint.RefPointLocation))
            {
                evaluation = 3;
            }

            return evaluation;

        }

        public static double wedge_factor(int energy, int angle)
        {
            double factor = 1;
            switch (energy)
            {
                case 6:
                    switch (angle)
                    {
                        case 10:
                            factor = 0.95;
                            break;
                        case 15:
                            factor = 0.925;
                            break;
                        case 30:
                            factor = .85;
                            break;
                        case 45:
                            factor = .77;
                            break;
                        case 60:
                            factor = .655;
                            break;
                        default:
                            return factor;
                    }
                    break;
                case 18:
                    switch (angle)
                    {
                        case 10:
                            factor = 0.96;
                            break;
                        case 15:
                            factor = 0.945;
                            break;
                        case 30:
                            factor = .89;
                            break;
                        case 45:
                            factor = .82;
                            break;
                        case 60:
                            factor = .73;
                            break;
                        default:
                            return factor;
                    }
                    break;
            }
            return factor;

        }

        public static int mu_gy(double value, double target)
        {
            int evaluation = 1;
            if (value > target || Double.IsNaN(value))
            {
                evaluation = 3;
            }
            return evaluation;
        }
        public static int mu_gy(double value, double target, double tolerance)
        {
            int evaluation = 1;
            if (Math.Abs(value - target) > tolerance || Double.IsNaN(value))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int ref_point_number(int target, string value)
        {
            int evaluation = 1;
            if (target != Convert.ToInt16(value))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int dose_2_refpoint(ScriptContext context)
        {
            int evaluation = 1;
            if (Math.Abs(context.PlanSetup.PlannedDosePerFraction.Dose - context.PlanSetup.DosePerFraction.Dose)>0.01)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int lower_objective_oar(ScriptContext context, out string f_structure)
        {
            int evaluation = 1;
            f_structure = "";
            foreach (var structure in context.StructureSet.Structures.Where(n => !("ptv, ctv, gtv".Contains(n.DicomType.ToLower())) && !n.Id.ToLower().Contains("help")))
            {
                foreach (OptimizationPointObjective opo in context.PlanSetup.OptimizationSetup.Objectives.Where(n => n.StructureId == structure.Id).Where(n => n is OptimizationPointObjective))
                {
                    if (opo.Operator.ToString().ToLower().Contains("lower"))
                    {
                        evaluation = 3;
                        f_structure = structure.Id;
                    }

                }
            }
            return evaluation;
        }

        public static int scan_dibh(string image_id, int dibh)
        {
            int evaluation = 1;
            if (dibh == 2)
            {
                evaluation = 6;
            }
            else if (image_id.Contains("dibh")!= (dibh==1))
            {
                evaluation = 3;
            }

            return evaluation;
        }

        public static int dmax_in_target(ScriptContext context)
        {
            int evaluation = 1;
            if (!context.StructureSet.Structures.First(n => n.Id.Equals(context.PlanSetup.TargetVolumeID)).IsPointInsideSegment(context.PlanSetup.Dose.DoseMax3DLocation))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int plan_name_id(ScriptContext context)
        {
            int evaluation = 1;
            if (!context.PlanSetup.Id.Equals(context.PlanSetup.Name))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int mlc_at_max(double value, double target)
        {
            int evaluation = 1;
            if (value > target)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static double mlc_at_max_(ScriptContext context)
        {

            double mlcs_at_limit = 0;
            double total_mlcs = 0;
            foreach (var beam in context.PlanSetup.Beams.Where(n => n.IsSetupField == false))
            {

                foreach (var cp in beam.ControlPoints.Where(n => n.MetersetWeight > 0))
                {
                    if (cp.LeafPositions.Length > 4)
                    {
                        float[,] lp = cp.LeafPositions;
                        double min_x1 = lp[0, 0];
                        double max_x2 = lp[1, 0];
                        for (int n = 0; n <= lp.GetUpperBound(1); n++)
                        {

                            if (lp[0, n] < min_x1)
                            {
                                min_x1 = lp[0, n];
                            }
                            if (lp[1, n] > max_x2)
                            {
                                max_x2 = lp[1, n];
                            }
                        }
                        for (int n = 0; n <= lp.GetUpperBound(1); n++)
                        {


                            if (Math.Abs(lp[0, n] - min_x1) > 149.9 && Math.Abs(lp[0,n]-lp[1,n])>0.05)
                            {
                                mlcs_at_limit++;

                            }

                            if (Math.Abs(max_x2 - lp[1, n]) > 149.9 && Math.Abs(lp[0, n] - lp[1, n]) > 0.05)
                            {
                                mlcs_at_limit++;


                            }
                            total_mlcs = total_mlcs + 2;

                        }

                    }
                }

            }

            double result = mlcs_at_limit / total_mlcs * 100;
            return result;
        }

        public static int normalization(double norm_amount, string target, string volume, ScriptContext context)
        {
            int evaluation = 1;

            string method = context.PlanSetup.PlanNormalizationMethod;
            string target_volume = context.PlanSetup.TargetVolumeID;
            switch (volume)
            {
                case "dmean":
                    if (!(method.ToLower().Contains("target mean") && target_volume.ToLower().Contains(target)))
                    {
                        evaluation = 3;
                    }
                    else if (Convert.ToDouble(Regex.Match(method, @"^\d+").Value) != norm_amount)
                    {
                        evaluation = 3;
                    }
                    break;
                case "dmin":
                    if (!(method.ToLower().Contains("target minimum") && target_volume.ToLower().Contains(target)))
                    {
                        evaluation = 3;
                    }
                    else if (Convert.ToDouble(Regex.Match(method, @"^\d+").Value) != norm_amount)
                    {
                        evaluation = 3;
                    }
                    break;
                default:
                    
                    if (!(norm_amount.ToString() + ".00% covers " + volume + ".00% of Target Structure").ToLower().Equals(context.PlanSetup.PlanNormalizationMethod.ToLower(), StringComparison.OrdinalIgnoreCase))
                    {
                        evaluation = 3;
                    }

                    break;
            }


            return evaluation;
        }

        public static int obi_angle(double target, ScriptContext context, out string field_name)
        {
            int evaluation = 1;
            field_name = "none";
            if (context.PlanSetup.Beams.Count(n => n.IsSetupField == true && !n.Id.ToLower().Contains("cbct") && n.ControlPoints.First().GantryAngle == target) == 0)
            {
                evaluation = 3;
            }
            else
            {
                field_name = context.PlanSetup.Beams.First(n => n.IsSetupField == true && !n.Id.ToLower().Contains("cbct") && n.ControlPoints.First().GantryAngle == target).Id;
            }

            return evaluation;
        }

        public static int normal_tissue_objective(ScriptContext context, out string norm_press)
        {
            int evaluation = 3;
            norm_press = null;
            foreach (OptimizationObjective par in context.PlanSetup.OptimizationSetup.Objectives)
            {
                if (par is OptimizationPointObjective)
                {
                    OptimizationPointObjective ntp = par as OptimizationPointObjective;
                    if (norm_press == null && ntp.Operator.ToString().ToLower().Contains("upper") && ntp.StructureId.ToLower().Contains("ring") && ntp.Priority > 0)
                    {
                        norm_press = norm_press + "Ring";
                        evaluation = 1;
                    }
                }

            }

            foreach (OptimizationParameter par in context.PlanSetup.OptimizationSetup.Parameters)
            {
                if (par is OptimizationNormalTissueParameter)
                {
                    OptimizationNormalTissueParameter ntp = par as OptimizationNormalTissueParameter;
                    if (ntp.Priority > 0)
                    {


                        if (norm_press == null)
                        {
                            norm_press = "NTO";
                            evaluation = 1;
                        }
                        else
                        {
                            norm_press = norm_press + " and NTO";
                            evaluation = 1;
                        }
                    }
                }
            }

            return evaluation;
        }

        public static int mlc_at_jaw(IEnumerable<Beam> beams, string mlc,double static_limit, double dyn_limit, out double distance, out double fraction)
        {
            
            int evaluation = 1;
            double leaf_too_close = 0;
            double limit = 0;
            double total = 0;
            distance = static_limit;


            foreach (var beam in beams.Where(n => n.IsSetupField == false))
            {
                if (!beam.MLCPlanType.ToString().ToLower().Contains("static"))
                {
                    limit = dyn_limit;
                    
                }
                else
                {
                    limit = static_limit;
                }
                distance = limit;
                double x1 = beam.ControlPoints.Skip(1).First().JawPositions.X1;
                double x2 = beam.ControlPoints.Skip(1).First().JawPositions.X2;
                double y1 = beam.ControlPoints.Skip(1).First().JawPositions.Y1;
                double y2 = beam.ControlPoints.Skip(1).First().JawPositions.Y2;
                int begin = limit_leaf(mlc, y2, 2);
                int end = limit_leaf(mlc, y1, 1);
                foreach (var cp in beam.ControlPoints.Skip(1))
                {

                    for (int i = begin; i <= end; i++)
                    {
                        total=total+2;
                        if ((x1 - cp.LeafPositions[0, i]) < limit && (x1 - cp.LeafPositions[0, i]) >-.5 && Math.Abs(cp.LeafPositions[0, i] - cp.LeafPositions[1, i]) > 0.05)
                        {
                            leaf_too_close++;
                            if(Math.Abs(x1 - cp.LeafPositions[0, i]) < distance)
                            {
                                distance = Math.Abs(x1 - cp.LeafPositions[0, i]);
                            }
                        }
                        if (Math.Abs(x2 - cp.LeafPositions[1, i]) < limit && Math.Abs(cp.LeafPositions[0, i] - cp.LeafPositions[1, i]) > 0.05)
                        {
                            leaf_too_close++;
                            if (Math.Abs(x2 - cp.LeafPositions[1, i]) < distance)
                            {
                                distance = Math.Abs(x2 - cp.LeafPositions[1, i]);
                            }
                        }
                    }
                }

            }
            distance = Math.Round(distance, 1);
            fraction = Math.Round(leaf_too_close / total * 100, 1);
            if (fraction > 0.01) { evaluation = 3; }
            return evaluation;
        }

        private static int limit_leaf(string mlc, double y, int direction)
        {
            int leaf = 0 ;
            
            if (mlc.ToLower().Contains("millennium"))
            {
                
                if (Math.Abs(y) <= 100)
                {
                    leaf = -(int)Math.Ceiling(y / 5)+30;
                }
                else if (Math.Abs(y) > 100)
                {
                    leaf = -(int)Math.Ceiling((y - 100 * (2 * direction - 3)) / 10) + 50*(2-direction) + 10*(direction-1);
                }
            }
            else if (mlc.ToLower().Contains("high definition"))
            {
                
                if (Math.Abs(y) <= 40)
                {
                    leaf = -(int)Math.Ceiling(y / 2.5) + 30;
                }
                else if (Math.Abs(y) > 40)
                {
                    leaf = -(int)Math.Ceiling((y - 40 * (2 * direction - 3)) / 5) + 46 * (2 - direction) + 14 * (direction - 1);
                }
            }
            else
            {
                MessageBox.Show($"{mlc} is undefined!");
            }


            return leaf;
        }

        public static int plan_norm_value(ScriptContext context, double low, double high)
        {
            int evaluation = 1;
            if (context.PlanSetup.PlanNormalizationValue < low || context.PlanSetup.PlanNormalizationValue > high)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        public static int use_gated(bool useGating, bool v1)
        {
            int evaluation = 1;
            if (useGating != v1)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int refpoint_limit(double dose, double session_limit)
        {
            int evaluation = 1;
            if (Math.Round(dose, 3) != Math.Round(session_limit, 3))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int User_origin_in_body(VVector userOrigin, Structure structure)
        {
            int evaluation = 1;
            if (!structure.IsPointInsideSegment(userOrigin))
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int same_isocenter_tbi(double target_y, double target_z, ScriptContext context)
        {
            int evaluation = 1;
            

            if ( context.PlanSetup.Beams.Where(n => n.IsocenterPosition.y == target_y).Count() != context.PlanSetup.Beams.Count() || context.PlanSetup.Beams.Where(n => n.IsocenterPosition.z == target_z).Count() != context.PlanSetup.Beams.Count())
            {
                evaluation = 3;
            }
                        
            return evaluation;
        }

        internal static int x_isocenter(double target_x, double x)
        {
            int evaluation = 1;


            if (Math.Abs(Math.Round(x))!=target_x)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        internal static string tbi_coll_angle(IEnumerable<Beam> beams)
        {
            string evaluation = null;

            if (beams.Count(n=> n.ControlPoints.First().CollimatorAngle<20|| n.ControlPoints.First().CollimatorAngle < 200 && n.ControlPoints.First().CollimatorAngle >160 || n.ControlPoints.First().CollimatorAngle > 340) > 0)
            {
                evaluation = beams.First(n => n.ControlPoints.First().CollimatorAngle < 20 || n.ControlPoints.First().CollimatorAngle < 200 || n.ControlPoints.First().CollimatorAngle > 160 || n.ControlPoints.First().CollimatorAngle > 340).Id;
            }


            return evaluation;
        }

        internal static int tbi_gantry_angle(Beam beam, double limit )
        {
            int evaluation = 3;

            if (beam.ControlPoints.First().GantryAngle == limit )
            {
                evaluation = 1;
            }


            return evaluation;
        }

        internal static int tbi_max_mu_per_field(Beam beam, double field_mu_limit)
        {
            int evaluation = 1;

            if (beam.Meterset.Value > field_mu_limit)
            {
                evaluation = 3;
            }

            return evaluation;
        }

        internal static int tbi_doserate(int doseRate, double field_doserate_limit)
        {
            int evaluation = 1;

            if (doseRate != field_doserate_limit)
            { evaluation = 3; }
            return evaluation;
        }

        internal static int treatment_time(double treatmentTime, double time_limit)
        {
            int evaluation = 1;
            if (treatmentTime > time_limit)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int tbi_spoiler_exist(Structure spoiler)
        {
            int evaluation = 1;
            if (spoiler == null)
            {
                evaluation = 3;

            }
            return evaluation;
        }

        internal static int tbi_spoiler_in_body(Structure spoiler, Structure body)
        {
            int evaluation = 1;
            if (!body.IsPointInsideSegment(spoiler.CenterPoint))
            {
                evaluation = 3;
            
            }
            return evaluation;
        }

        internal static int tbi_spoiler_water(Structure spoiler)
        {
            int evaluation = 1;
            spoiler.GetAssignedHU(out double HU);
            if (HU < 0 || HU > 10)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int fff(bool fff, Beam beam)
        {
            int evaluation = 1;
            if (beam.EnergyModeDisplayName.ToLower().Contains("fff") != fff)
            {
                evaluation = 3;
            }
            return evaluation;
        }

        internal static int lowdoserate(bool low_doserate, Beam beam)
        {
            int evaluation = 1;
            switch (low_doserate)
            {
                case true:
                    if (beam.DoseRate > 800) { evaluation = 3; }
                    break;
                case false:
                    if (beam.DoseRate <= 800) { evaluation = 3; }
                    break;
            }
            return evaluation;  

        }

        internal static int arc_collision(double x, double top_y, double y, IEnumerable<Beam> beams,double mid_x)
        {
            int evaluation = 3;
            if (-y + top_y < -.0156 * x * x + 0.0128 * x + 273.4)
            {
                evaluation = 1;
            }
            else
            {
                foreach (Beam beam in beams.Where(n=> n.Technique.Id.ToLower().Contains("arc")))
                {
                    if ((beam.ControlPoints.Any(m=> m.GantryAngle > 181) && x > mid_x) || (beam.ControlPoints.Any(m => m.GantryAngle < 179) && x < mid_x))
                    {
                        evaluation = 3;
                    }
                    if (!(beam.ControlPoints.First().PatientSupportAngle>45 && beam.ControlPoints.First().PatientSupportAngle < 180) && beam.ControlPoints.Any(m=> m.GantryAngle>181 && m.GantryAngle<345))
                    {
                        evaluation = 3;
                    }
                    if (!(beam.ControlPoints.First().PatientSupportAngle < 315 && beam.ControlPoints.First().PatientSupportAngle > 180) && beam.ControlPoints.Any(m => m.GantryAngle < 179 && m.GantryAngle > 15))
                    {
                        evaluation = 3;
                    }
                }
            }
            return evaluation;
        }

        internal static int treat_name_couch_angle(Beam beam)
        {
            int evaluation = 3;
            if (beam.Id.Contains(beam.PatientSupportAngleToUser(beam.ControlPoints.First().PatientSupportAngle).ToString()))
            {
                evaluation = 1;
            }
            return evaluation;
        }

        internal static int ET_max_mu_per_field(int max_mu, Beam beam)
        {
            int evaluation = 1;

            if (beam.Meterset.Value > max_mu)
            {
                evaluation = 3;
            }

            return evaluation;
        }


    }
}

