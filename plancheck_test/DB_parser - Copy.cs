using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Data.SqlClient;


namespace VMS.TPS
{
    public class DB_parser_copy
    {
        public static class DB_diag
        {
            public static int DB_diag_code { get; set; }
            public static int DB_fraktioner { get; set; }
            public static double DB_fx_dosis { get; set; }
            public static double DB_norm_procent { get; set; }
            public static string DB_norm_volume { get; set; }
            public static string DB_norm_target { get; set; }
            public static string DB_setup { get; set; }          
        }

        public static class DB_structures
        {
            public static int[] DB_strukt;
        }

        public static class DB_constraints
        {
            public static string type = "";
            public static string DB_navn = null;
            public static bool DB_req = false;
            public static double DB_dmean = double.NaN;
            public static double DB_rel_vol_dose = double.NaN;
            public static double DB_rel_vol = double.NaN;
            public static double DB_abs_vol_dose = double.NaN;
            public static double DB_abs_vol = double.NaN;
            public static int DB_mere_end_mindre_end = 0;
            public static string DB_trafik_lys = null;
            public static string DB_kommentar = null;
        }

        public bool Read_diagnose(string diag_code, string hospital)
        {

            bool db_result = false;
            string connStr = "****;";
            SqlConnection SqlConnection = new SqlConnection(connStr);
            try
            {

                SqlConnection.Open();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            SqlCommand cmd = new SqlCommand();
            try
            {
                SqlDataReader myReader = null;
                SqlCommand myCommand = new SqlCommand($"SELECT * FROM Plan_Diagnose_{hospital} WHERE diag_code='{diag_code.ToUpper()}';", SqlConnection); //Sådan ser en select ud
                
                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {

                    if (!myReader.IsDBNull(0) || !myReader.IsDBNull(2) || !myReader.IsDBNull(3))
                    {
                        DB_diag.DB_diag_code = myReader.GetInt32(0);
                        DB_diag.DB_fraktioner = myReader.GetInt32(2);
                        DB_diag.DB_fx_dosis = myReader.GetDouble(3);
                        if (!myReader.IsDBNull(4))
                        {
                            DB_diag.DB_norm_procent = myReader.GetDouble(4);
                        }
                        else
                        {
                            DB_diag.DB_norm_procent = 100;
                        }
                        if (!myReader.IsDBNull(5))
                        {
                            DB_diag.DB_norm_volume = myReader.GetString(5);

                        }
                        else
                        {
                            DB_diag.DB_norm_volume = "dmean";
                        }
                        if (!myReader.IsDBNull(6))
                        {
                            DB_diag.DB_norm_target = myReader.GetString(6);
                        }
                        else
                        {
                            DB_diag.DB_norm_target = "ptv";
                        }
                        if (!myReader.IsDBNull(7))
                        {
                            DB_diag.DB_setup = myReader.GetString(7);
                        }
                        else
                        {
                            DB_diag.DB_setup = "cbct";
                        }
                        db_result = true;
                    }

                    else
                    {
                        System.Windows.MessageBox.Show("Manglende værdi i diagnose tabel.");
                        return db_result;
                    }

                }



            }
            catch (Exception db_e)
            {
                System.Windows.MessageBox.Show(db_e.ToString());
                db_result = false;
            }
            SqlConnection.Close();
            //System.Windows.MessageBox.Show("Done.");
            return db_result;
        }




        public bool Read_priority(int diag_code, string hospital)
        {
            // Der skal laves en opførsel for 000 koder
            bool db_result = false;
            string connStr = "****;";
            SqlConnection SqlConnection = new SqlConnection(connStr);
            try
            {
                //System.Windows.MessageBox.Show("Connecting to MySQL...");
                SqlConnection.Open();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }
            SqlCommand cmd = new SqlCommand();
            try
            {
                SqlDataReader myReader = null;

                SqlCommand myCommand = new SqlCommand($"SELECT COUNT(idStruktur) FROM Plan_Prioritet_{hospital} WHERE idDiagnose_kode='{diag_code}';", SqlConnection); //Sådan ser en select ud
                myReader = myCommand.ExecuteReader();

                int number_of_constraints = 0;

                while (myReader.Read())
                {
                    number_of_constraints = myReader.GetInt32(0);
                }
                myReader.Close();

                if (number_of_constraints == 0)
                {
                    //System.Windows.MessageBox.Show("Der er ingen constraints til denne diagnosekode.");
                    int[] structure_index = new int[0];
                    db_result = true;
                }
                else
                {
                    myCommand = new SqlCommand($"SELECT * FROM Plan_Prioritet_{hospital} WHERE idDiagnose_kode='{diag_code}';", SqlConnection); //Sådan ser en select ud
                    myReader = myCommand.ExecuteReader();
                    int[] structure_index = new int[number_of_constraints];
                    int i = 0;
                    while (myReader.Read())
                    {
                        structure_index[i++] = myReader.GetInt32(2);
                    }

                    DB_structures.DB_strukt = structure_index;
                    db_result = true;

                }



            }

            catch (Exception db_e)
            {
                System.Windows.MessageBox.Show(db_e.ToString());
                db_result = false;
            }
            SqlConnection.Close();
            //System.Windows.MessageBox.Show("Done.");
            return db_result;


        }



        public bool Read_structure(int struktur, string hospital)
        {

            bool db_result = false;

            string connStr = "****;";
            SqlConnection SqlConnection = new SqlConnection(connStr);
            try
            {
                //System.Windows.MessageBox.Show("Connecting to MySQL...");
                SqlConnection.Open();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
            }


            SqlCommand cmd = new SqlCommand();
            try
            {

                SqlDataReader myReader;

                SqlCommand myCommand = new SqlCommand($"SELECT * FROM Plan_Struktur_{hospital} WHERE idstruktur_tabel='{struktur}';", SqlConnection); //Sådan ser en select ud

                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    if (!myReader.IsDBNull(1))
                    {
                        DB_constraints.DB_navn = myReader.GetString(1);

                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Der er ikke angivet noget navn på constraint id: " + struktur.ToString());
                    }

                    if (!myReader.IsDBNull(2) && myReader.GetInt32(2) < 2)
                    {
                        if (myReader.GetInt32(2) == 1)
                        {
                            DB_constraints.DB_req = true;
                        }
                        else
                        {
                            DB_constraints.DB_req = false;
                        }

                    }
                    else
                    {
                        DB_constraints.type = "rapport";

                    }

                    if (!myReader.IsDBNull(3) && myReader.GetDouble(3) != 0)
                    {
                        DB_constraints.DB_dmean = myReader.GetDouble(3);
                        DB_constraints.type = "dmean";


                    }
                    else
                    {
                        DB_constraints.DB_dmean = double.NaN;

                    }

                    if (!myReader.IsDBNull(4) && myReader.GetDouble(4) != 0)
                    {
                        DB_constraints.DB_rel_vol_dose = myReader.GetDouble(4);
                        DB_constraints.type = "rel_vol";

                    }
                    else
                    {
                        DB_constraints.DB_rel_vol_dose = double.NaN;
                    }

                    if (!myReader.IsDBNull(5))
                    {
                        DB_constraints.DB_rel_vol = myReader.GetDouble(5);
                    }
                    else
                    {
                        DB_constraints.DB_rel_vol = double.NaN;
                    }

                    if (!myReader.IsDBNull(6) && myReader.GetDouble(6) != 0)
                    {
                        DB_constraints.DB_abs_vol_dose = myReader.GetDouble(6);
                        DB_constraints.type = "abs_vol";

                    }
                    else
                    {
                        DB_constraints.DB_abs_vol_dose = double.NaN;
                    }

                    if (!myReader.IsDBNull(7))
                    {
                        DB_constraints.DB_abs_vol = myReader.GetDouble(7);
                    }
                    else
                    {
                        DB_constraints.DB_abs_vol = double.NaN;
                    }

                    if (!myReader.IsDBNull(8))
                    {
                        DB_constraints.DB_mere_end_mindre_end = myReader.GetInt32(8);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Der er ikke angivet noget navn på constraint id: " + struktur.ToString());
                    }


                    DB_constraints.DB_trafik_lys = myReader.GetString(9);

                    if (DB_constraints.DB_mere_end_mindre_end == 9)
                    {
                        DB_constraints.type = "manual";
                        if (!myReader.IsDBNull(10))
                        {
                            DB_constraints.DB_kommentar = myReader.GetString(10);
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Der er ikke angivet kommentar på manuelt check: " + DB_constraints.DB_navn);
                        }
                    }
                    try
                    {


                        if (DB_constraints.type.Length < 1)
                        {
                            MessageBox.Show("Der er ingen type på constraint" + Environment.NewLine + myReader.GetString(1));
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Der er ingen type på constraint" + Environment.NewLine + myReader.GetString(1) + Environment.NewLine + e.ToString());
                    }


                }

                db_result = true;


            }
            catch (Exception db_e)
            {
                System.Windows.MessageBox.Show(db_e.ToString());
            }

            return db_result;

        }


        public bool Output_check( string course, string plan_name, string user_ID, string file_name, string anon_ID, string hospital)
        {

            bool db_result = false;
            string connStr = "*****;";
            using (SqlConnection SqlConnection = new SqlConnection(connStr))
            {

                try
                {
                    string add_string = $"INSERT into Plan_data_{hospital} ( Anon_ID, Course, Eclipseplan, Time, EclipseUser, File_Name_html) VALUES ( @anon_ID , @course , @plan_name , @time , @user_ID , @file_name )"; //Sådan ser en select ud
                    using (SqlCommand myCommand = new SqlCommand(add_string, SqlConnection))
                    {

                        
                        myCommand.Parameters.AddWithValue("@anon_ID", anon_ID);
                        myCommand.Parameters.AddWithValue("@course", course);
                        myCommand.Parameters.AddWithValue("@plan_name", plan_name);
                        myCommand.Parameters.AddWithValue("@time", DateTime.Now);
                        myCommand.Parameters.AddWithValue("@user_ID", user_ID);
                        myCommand.Parameters.AddWithValue("@file_name", file_name);

                        SqlConnection.Open();
                        int result = myCommand.ExecuteNonQuery();

                        if (!(result < 0))
                        {
                            db_result = true;
                        }

                    }
                }

                catch (Exception db_e)
                {
                    System.Windows.MessageBox.Show(db_e.ToString());
                    db_result = false;
                }
            }

            return db_result;
        }

        public bool output_results( string anon_ID, string course, string plan_name, string user_ID, string test, string hospital)
        {

            bool db_result = false;
            string connStr = "****;";
            using (SqlConnection SqlConnection = new SqlConnection(connStr))
            {

                try
                {
                    string add_string = $"INSERT into Plan_results_{hospital} ( Anon_ID, Course, Eclipseplan, Time, EclipseUSer, Test) VALUES (  @anon_ID , @course , @plan_name , @time , @user_ID , @test )"; //Sådan ser en select ud
                    using (SqlCommand myCommand = new SqlCommand(add_string, SqlConnection))
                    {

                        
                        myCommand.Parameters.AddWithValue("@anon_ID", anon_ID);
                        myCommand.Parameters.AddWithValue("@course", course);
                        myCommand.Parameters.AddWithValue("@plan_name", plan_name);
                        myCommand.Parameters.AddWithValue("@time", DateTime.Now);
                        myCommand.Parameters.AddWithValue("@user_ID", user_ID);
                        myCommand.Parameters.AddWithValue("@test", test);

                        SqlConnection.Open();
                        int result = myCommand.ExecuteNonQuery();

                        if (!(result < 0))
                        {
                            db_result = true;
                        }

                    }
                }

                catch (Exception db_e)
                {
                    System.Windows.MessageBox.Show(db_e.ToString());
                    db_result = false;
                }
            }

            return db_result;
        }

        public bool <f( string anon_ID, string course, string plan_name, string user_ID, string test_ID, int status, string hospital)
        {
            bool fail = true;
            if (status == 1)
            {
                fail = false;
            }

            int failures = 0;
            int total = 0;
            bool db_result = false;
            string connStr = "****;";
            using (SqlConnection SqlConnection = new SqlConnection(connStr))
            {
                try
                {

                    SqlConnection.Open();

                    try
                    {
                        string get_string = $"SELECT TOP 1 Fail, Total FROM Plan_Summary_Artikel_{hospital} WHERE Test ='{test_ID} ' ORDER BY id DESC";
                        using (SqlCommand myCommand = new SqlCommand(get_string, SqlConnection))
                        {
                            
                            SqlDataReader reader = myCommand.ExecuteReader();
                            
                            
                                while (reader.Read())
                                {
                                    failures = reader.GetInt32(0);
                                    total = reader.GetInt32(1);
                               
                                }

                            reader.Close();
                        }
                        
                        string add_string = $"INSERT into Plan_Summary_Artikel_{hospital} (Test, Time, Fail, Total) VALUES (  @test , @time , @fail , @total)"; //Sådan ser en select ud
                        using (SqlCommand myCommand = new SqlCommand(add_string, SqlConnection))
                        {
                            myCommand.Parameters.AddWithValue("@test", test_ID);
                            myCommand.Parameters.AddWithValue("@time", DateTime.Now);
                            if (fail)
                            {
                                failures = failures+1;
                                output_results( anon_ID, course, plan_name, user_ID, test_ID, hospital);
                            }
                            total = total+1;
                            myCommand.Parameters.AddWithValue("@fail", failures);
                            myCommand.Parameters.AddWithValue("@total", total);
                            
                            int result = myCommand.ExecuteNonQuery();

                            if (!(result < 0))
                            {
                                db_result = true;
                            }
                        }
                    }

                    catch (Exception db_e)
                    {
                        System.Windows.MessageBox.Show(db_e.ToString());
                        db_result = false;
                    }
                }
                catch (Exception db_e)
                {
                    System.Windows.MessageBox.Show(db_e.ToString());
                }
                SqlConnection.Close();
                return db_result;
                
            }
        }
    }
}
