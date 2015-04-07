using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows;
using SpreadsheetGear;
using System.Collections.Generic;

namespace Ming.Tools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Data

        const string CONN_STRING = @"Initial Catalog=fa;Data Source=dev-db2; Integrated Security=SSPI; Max Pool Size = 75000";

        int[] _eventID;
        int[] _year;

        #endregion




        public MainWindow()
        {
            InitializeComponent();
        }


        private void btnBuildDataUsingSpreadSheetGear_Click(object sender, RoutedEventArgs e)
        {
            BuildDataUsingSpreadSheetGear(txtFilename.Text);
            SaveDataToSql();
        }



        private void btnBuildDataFromSql_Click(object sender, RoutedEventArgs e)
        {
            BuildDataFromSql();
            SaveDataToSql();
        }


        private void btnReadData_Click(object sender, RoutedEventArgs e)
        {
            ReadDataFromSql();
        }




        void BuildDataFromSql()
        {
            // saving an array into database as one binary field
            var conn = new SqlConnection(CONN_STRING);

            DataSet dtset = new DataSet();
            //var command = new SqlDataAdapter("select Year, EventID from [tblMasterYlt_Air]", conn);
            var command = new SqlDataAdapter("select Year, EventID from [tblMasterYlt_Eqe]", conn);
            command.Fill(dtset);

            DataTable table = dtset.Tables[0];

            int size = table.Rows.Count;

            _eventID = new int[size];
            _year = new int[size];

            for (int i = 0; i < size; i++)
            {
                _year[i] = Convert.ToInt32(table.Rows[i][0]);
                _eventID[i] = Convert.ToInt32(table.Rows[i][1]);
            }

        }



        void BuildDataFromTextFile(string filename)
        {
            var allData = System.IO.File.ReadAllText(filename);

            //string[] lines = allData.Split((Environment.NewLine + Environment.NewLine).ToCharArray());
            string[] lines = allData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

            // skip the first row
            var lstYear = new List<int>();
            var lstEvent = new List<int>();


            for (int i = 0; i < lines.Length; i++)
            {
                string[] cols = lines[i].Split(',');

                // get year and eventid
                if (cols.Length == 4)
                {
                    int year, eventid;
                    if (int.TryParse(cols[0], out year) && int.TryParse(cols[2], out eventid)) {
                        lstYear.Add(year);
                        lstEvent.Add(eventid);
                    }
                }
            }

            _year = lstYear.ToArray();
            _eventID = lstEvent.ToArray();

        }




        //void BuildDataFromExcel(string filename)
        //{
        //    // saving an array into database as one binary field

        //    var conn = new OleDbConnection(string.Format("provider=Microsoft.Jet.OLEDB.4.0;Data Source='{0}';Extended Properties=Excel 8.0;", filename));

        //    DataSet dtset = new DataSet();
        //    var command = new OleDbDataAdapter("select * from [EQE YLT EQ EAST$]", conn);
        //    command.Fill(dtset);

        //    DataTable table = dtset.Tables[0];

        //    int size = table.Rows.Count;

        //    _eventID = new int[size];
        //    _year = new int[size];

        //    for (int i = 0; i < size; i++)
        //    {
        //        _eventID[i] = Convert.ToInt32(table.Rows[i][0]);
        //        _year[i] = Convert.ToInt16(table.Rows[i][1]);
        //    }

        //    // save the data into sql database
        //    MessageBox.Show(string.Format("Model:{0};  Peril:{1}", txtModel.Text, txtPeril.Text));

        //}



        void BuildDataUsingSpreadSheetGear(string filename)
        {
            // saving an array into database as one binary field
            IWorkbook wb = SpreadsheetGear.Factory.GetWorkbook(filename);

            // get the first sheet
            IWorksheet sh = wb.Worksheets[0];

            object[,] dat = (object[,])sh.UsedRange.Value;

            int size = dat.GetUpperBound(0);        // do not add 1, need to ignore the first row.


            _eventID = new int[size];
            _year = new int[size];

            for (int i = 0; i < size; i++)
            {
                _year[i] = Convert.ToInt32(dat[i + 1, 0]);
                _eventID[i] = Convert.ToInt32(dat[i+1, 1]);
            }

            // save the data into sql database
            MessageBox.Show(string.Format("Model:{0};  Peril:{1}", txtModel.Text, txtPeril.Text));

        }


        void SaveDataToSql()
        {
            // connect to fa databae
            var sqlconn = new SqlConnection(CONN_STRING);
            sqlconn.Open();

            var da = new SqlDataAdapter("SELECT TOP 0 Model, Peril, Trial, Year, EventID from [tblMasterYlt]", sqlconn);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            da.InsertCommand = cb.GetInsertCommand();

            var tb = new DataTable();
            da.Fill(tb);

            // Add one row
            DataRow row = tb.NewRow();
            row["Model"] = txtModel.Text;
            row["Peril"] = txtPeril.Text;
            row["Trial"] = Convert.ToInt32(txtTrial.Text);
            row["Year"] = GetByteArrayFromIntArray(_year);
            row["EventID"] = GetByteArrayFromIntArray(_eventID);
            tb.Rows.Add(row);
            int results = da.Update(tb);

            MessageBox.Show(results.ToString());
        }


        void ReadDataFromSql()
        {
            const string SQL = "SELECT Model, Peril, Year, EventID, Freq from [tblMasterYlt] WHERE MODEL='{0}' AND PERIL='{1}'";


            // connect to fa databae
            var sqlconn = new SqlConnection(CONN_STRING);
            sqlconn.Open();

            var da = new SqlDataAdapter(string.Format(SQL, txtModel.Text, txtPeril.Text), sqlconn);

            var tb = new DataTable();
            da.Fill(tb);

            // Add one row
            DataRow row = tb.Rows[0];
            byte[] year = (byte[])row["Year"];
            byte[] eventid = (byte[])row["EventID"];

            _year = GetIntArrayFromByteArray(year);
            _eventID = GetIntArrayFromByteArray(eventid);

            MessageBox.Show("OK!");
        }



        public static byte[] GetByteArrayFromIntArray(int[] intArray)
        {
            byte[] data = new byte[intArray.Length * 4];
            for (int i = 0; i < intArray.Length; i++)
                Array.Copy(BitConverter.GetBytes(intArray[i]), 0, data, i * 4, 4);
            return data;
        }



        public static int[] GetIntArrayFromByteArray(byte[] byteArray)
        {
            int[] intArray = new int[byteArray.Length / 4];
            for (int i = 0; i < byteArray.Length; i += 4)
                intArray[i / 4] = BitConverter.ToInt32(byteArray, i);
            return intArray;
        }



        private void btnBuildDataFromTextFile_Click(object sender, RoutedEventArgs e)
        {
            BuildDataFromTextFile(txtFilename.Text);
            SaveDataToSql();
        }




    }
}
