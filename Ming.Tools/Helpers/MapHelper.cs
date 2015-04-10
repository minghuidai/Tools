using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.SpatialToolbox;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Ming.Tools.Helpers
{
    public static class MapHelper
    {

        //public const string BingMapKey = "Ai6zQ5AwxFAZKY3DtRmKAPJHZVlK4h_e01jNbblWGbagsXzwH0nf5vYrTEr13kBd";


        /// <summary>
        /// Check if the specified location is inside the polygon.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static bool IsInPolygon(MapPolygon polygon, Location location) {

            bool inPoly = false;
            var points = polygon.Locations;
            var j = points.Count - 1;
            var lat = location.Latitude;
            var lon = location.Longitude;

            for (int i = 0; i < polygon.Locations.Count; i++)
            {
                if (points[i].Longitude < lon && points[j].Longitude >= lon || points[j].Longitude < lon && points[i].Longitude >= lon)
                {
                    if (points[i].Latitude + (lon - points[i].Longitude) / (points[j].Longitude - points[i].Longitude) * (points[j].Latitude - points[i].Latitude) < lat)
                    {
                        inPoly = !inPoly;
                    }
                }
                j = i;
            }
 
            return inPoly;
        }




        /// <summary>
        /// Create an mark used on the map
        /// </summary>
        /// <returns></returns>
        public static UIElement CreateLocMark(string color, int size = 4)
        {
            var converter = new ColorConverter();
            return CreateMark((Color)converter.ConvertFrom(color), size);
        }



        /// <summary>
        /// Create a location mark on the map
        /// </summary>
        /// <param name="color"></param>
        /// <param name="size"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static UIElement CreateMark(Color color, int size = 4, string label = null)
        {
            var mark = new Ellipse();
            mark.Width = size;
            mark.Height = size;
            mark.Fill = new SolidColorBrush(color);

            if (!string.IsNullOrEmpty(label))
            {
                mark.ToolTip = new ToolTip()
                {
                    Content = label,
                };
            }
            return mark;
        }




        /// <summary>
        /// Create a polygon
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        public static MapPolygon CreatePolygon(LocationCollection locations, string fillColor, string borderColor, double opacity = 1)
        {
            var converter = new BrushConverter();
            var polygon = new MapPolygon() {
                Locations = locations,
                Fill = (SolidColorBrush)converter.ConvertFrom(fillColor),
                Stroke = (SolidColorBrush)converter.ConvertFrom(borderColor),
                StrokeThickness = 1,
                Opacity = opacity,
            };
            return polygon;
        }





        /// <summary>
        /// Create a polygon
        /// </summary>
        /// <param name="locations"></param>
        /// <returns></returns>
        public static MapPolygon CreatePolygon(CoordinateCollection coordinateColl, string fillColor, string borderColor, double opacity = 1)
        {
            if (coordinateColl == null) throw new ArgumentException();


            // convert coordinate collection to maplocation
            var locations = new LocationCollection();

            foreach (var coor in coordinateColl) {
                locations.Add(new Location(coor.Latitude, coor.Longitude));
            }

            return CreatePolygon(locations, fillColor, borderColor, opacity);

        }





        /// <summary>
        /// Dusplicate a polygon
        /// </summary>
        /// <param name="srcPolygon"></param>
        /// <returns></returns>
        public static MapPolygon DuplicatePolygon(MapPolygon srcPolygon)
        {
            var newPlgn = new MapPolygon()
            {
                //Fill = SolidColorBrush(Color.FromRgb(255, 0, 0),
                //Stroke = srcPolygon.Stroke.Clone(),
                //StrokeThickness = srcPolygon.StrokeThickness,
                Opacity = 1,
            };

            var locs = new LocationCollection();
            foreach (var loc in srcPolygon.Locations)
            {
                locs.Add(new Location(loc.Latitude, loc.Longitude));
            }

            newPlgn.Locations = locs;


            return newPlgn;
        }



    }
}
