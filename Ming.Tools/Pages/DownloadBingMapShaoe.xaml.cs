using Microsoft.Maps.MapControl.WPF;
using Ming.Tools.Helpers;
using System;
using System.Collections.Generic;
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
using TMTech.MapService;

namespace Ming.Tools.Pages
{

    public partial class DownloadBingMapShaoe : Page
    {

        #region Data

        private static string _BingMapSessionID;

        public enum ActionStatus
        {
            None,
            CreatingPolygon,
            QueryBoundries,
        }

        private double _PolygonOpacity = 0.5;
        private string _PolygonBorderColor = "#fd7400";
        private string _LocationMarkColor = "#ff0000";
        private string _PolygonFillColor_1 = "#046380";
        private string _PolygonFillColor_2 = "#e74c3c";
        private int _MarkSize = 4;

        private ActionStatus _Status;
        //private MapPolygon _PolygonIsCrerating;
        //private MapLayer _Marks;
        #endregion


        #region commands

        // Defines two commands used for button CreatePolygon and Finish Polygon
        public static readonly RoutedCommand StartPolygonCommand = new RoutedCommand();
        private static readonly RoutedCommand EndPolygonCommand = new RoutedCommand();

        #endregion


        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public DownloadBingMapShaoe()
        {
            InitializeComponent();

            // Get bingmap session ID;
            bingMap.CredentialsProvider.GetCredentials((c) =>
            {
                _BingMapSessionID = c.ApplicationId;
            });

            //bingMap.MouseDown += bingMap_MouseDown;

        }
        #endregion




        #region button event handlers



        /// <summary>
        /// Download region bounreies
        /// </summary>
        private void DrawBountry()
        {
            bingMap.Children.Clear();

            string[] text = ReadLines();

            var service = Factory.GetMapService(chkQuerySpatial.IsChecked != null && (bool)chkQuerySpatial.IsChecked ? _BingMapSessionID : null);

            string level = txtDataLevel.Text;

            var allLocations = new List<Location>();

            foreach (var line in text)
            {
                var points = service.GetBoundries(line, level);

                if (points != null)
                {
                    var p = MapHelper.CreatePolygon(points, _PolygonFillColor_1, _PolygonBorderColor, 0.5);

                    bingMap.Children.Add(p);

                    foreach (var l in p.Locations)
                    {
                        allLocations.Add(l);
                    }
                }

            }

            if (allLocations.Count == 0)
                MessageBox.Show("No bountry was found.");
            else
                bingMap.SetView(allLocations, new Thickness(30), bingMap.Heading);

        }





        /// <summary>
        /// REtrieve Boundries from Bing Map spatial service
        /// </summary>
        private void RetrieveBountry()
        {
            bingMap.Children.Clear();

            string[] text = ReadLines();

            var service = Factory.GetMapService(_BingMapSessionID);

            string level = txtDataLevel.Text;

            foreach (var line in text)
            {
                var points = service.GetBoundries(line, level);

                if (points != null)
                {
                    var p = MapHelper.CreatePolygon(points, _PolygonFillColor_1, _PolygonBorderColor, 0.5);
                }
            }
        }




        /// <summary>
        /// REtrieve Boundries from Bing Map spatial service
        /// </summary>
        private void CheckBountry()
        {
            string[] text = ReadLines();

            var service = Factory.GetMapService();

            string level = txtDataLevel.Text;

            foreach (var line in text)
            {
                var points = service.GetBoundries(line, level);

                if (points == null)
                {
                    txtNotFoundLocations.AppendText(line + Environment.NewLine);
                }
            }
        }



        // Read text from RichTextBox control
        private string[] ReadLines()
        {
            var textRange = new TextRange(txtLocations.Document.ContentStart, txtLocations.Document.ContentEnd);

            string[] text = textRange.Text.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            if (text.Length > 0)
            {
                for (int i = 0; i < text.Length; i++)
                    text[i] = text[i].Trim();
            }

            return text;
        }

        #endregion



        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            RetrieveBountry();
        }

        private void btnDrawBoundries_Click(object sender, RoutedEventArgs e)
        {
            DrawBountry();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckBountry();
        }

    }
}
