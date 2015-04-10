using Microsoft.Win32;
using SpreadsheetGear;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ming.Tools.Pages
{

    public partial class ConvertBinaryData : Page
    {

        #region Data

        const string CONN_STRING = @"Initial Catalog=fa;Data Source=dev-db2; Integrated Security=SSPI; Max Pool Size = 75000";

        int[] _eventID;
        int[] _year;

        DataTable _Data;

        #endregion




        public ConvertBinaryData()
        {
            InitializeComponent();
        }


        private void btnBuildDataUsingSpreadSheetGear_Click(object sender, RoutedEventArgs e)
        {
            BuildDataUsingSpreadSheetGear(txtFilename.Text);
        }



        private void btnBuildDataFromSql_Click(object sender, RoutedEventArgs e)
        {
            BuildDataFromSql();
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





        /// <summary>
        /// Used for AIR US_TOH, and Get rid of the first chars in the event string
        /// </summary>
        /// <param name="filename"></param>
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
                if (cols.Length == 3)
                {
                    int year, eventid;

                    // remove the '22' from event string
                    string eventStr = cols[2];
                    //eventStr = eventStr.Substring(2);   // do not remove '22' for AIR TOH

                    if (int.TryParse(cols[1], out year) && int.TryParse(eventStr, out eventid))
                    {
                        lstYear.Add(year);
                        lstEvent.Add(eventid);
                    }
                }
            }

            _year = lstYear.ToArray();
            _eventID = lstEvent.ToArray();

        }





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

            var da = new SqlDataAdapter("SELECT TOP 0 Model, Peril, DRVersion, Trial, Year, EventID from [tblMasterYlt]", sqlconn);
            SqlCommandBuilder cb = new SqlCommandBuilder(da);
            da.InsertCommand = cb.GetInsertCommand();

            var tb = new DataTable();
            da.Fill(tb);

            // Add one row
            DataRow row = tb.NewRow();
            row["Model"] = txtModel.Text;
            row["Peril"] = txtPeril.Text;
            row["DRVersion"] = txtDRVersion.Text;
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
            //SaveDataToSql();
        }


        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true) txtFilename.Text = dlg.FileName;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveDataToSql();
        }




    }
}
