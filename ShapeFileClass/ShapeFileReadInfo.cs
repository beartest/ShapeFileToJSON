using System;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Collections;
using System.Linq;
using System.Windows.Input;
using System.Windows;

namespace test
{
    public static class ExtensionMethods
    {
        public static SimpleMarkerSymbol DEFAULT_MARKER_SYMBOL = new SimpleMarkerSymbol()
        {
            Style = SimpleMarkerSymbol.SimpleMarkerStyle.Circle,
            Color = new SolidColorBrush(Colors.Red)
        };

        public static SimpleLineSymbol DEFAULT_LINE_SYMBOL = new SimpleLineSymbol()
        {
            Color = new SolidColorBrush(Colors.Red),
            Style = SimpleLineSymbol.LineStyle.Solid,
            Width = 2
        };

        public static SimpleFillSymbol DEFAULT_FILL_SYMBOL = new SimpleFillSymbol()
        {
            Fill = new SolidColorBrush(Colors.Red),
            BorderBrush = new SolidColorBrush(Colors.Green),
            BorderThickness = 2
        };

        public static Symbol GetDefaultSymbol(this ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            if (geometry == null)
                return null;

            Type t = geometry.GetType();
            if (t == typeof(MapPoint))
                return DEFAULT_MARKER_SYMBOL;
            else if (t == typeof(MultiPoint))
                return DEFAULT_MARKER_SYMBOL;
            else if (t == typeof(Polyline))
                return DEFAULT_LINE_SYMBOL;
            else if (t == typeof(Polygon))
                return DEFAULT_FILL_SYMBOL;
            else if (t == typeof(Envelope))
                return DEFAULT_FILL_SYMBOL;

            return null;
        }


        public static Graphic ToGraphic(this ShapeFileRecord record)
        {
            if (record == null)
                return null;

            Graphic graphic = new Graphic();
            //add all the attributes to the graphic
            foreach (var item in record.Attributes)
            {
                graphic.Attributes.Add(item.Key, item.Value);
            }

            //add the geometry to the graphic
            switch (record.ShapeType)
            {
                case (int)ShapeType.NullShape:
                    break;
                case (int)ShapeType.Point:
                    graphic.Geometry = GetPoint(record);
                    graphic.Symbol = DEFAULT_MARKER_SYMBOL;
                    break;
                case (int)ShapeType.Polygon:
                    graphic.Geometry = GetPolygon(record);
                    graphic.Symbol = DEFAULT_FILL_SYMBOL;
                    break;
                case (int)ShapeType.PolyLine:
                    graphic.Geometry = GetPolyline(record);
                    graphic.Symbol = DEFAULT_LINE_SYMBOL;
                    break;
                default:
                    break;
            }

            return graphic;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPolyline(ShapeFileRecord record)
        {
            Polyline line = new Polyline();
            for (int i = 0; i < record.NumberOfParts; i++)
            {
                // Determine the starting index and the end index
                // into the points array that defines the figure.
                int start = record.Parts[i];
                int end;
                if (record.NumberOfParts > 1 && i != (record.NumberOfParts - 1))
                    end = record.Parts[i + 1];
                else
                    end = record.NumberOfPoints;

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                // Add line segments to the polyline
                for (int j = start; j < end; j++)
                {
                    System.Windows.Point point = record.Points[j];
                    points.Add(new MapPoint(point.X, point.Y));
                }

                line.Paths.Add(points);
            }

            return line;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPolygon(ShapeFileRecord record)
        {
            Polygon polygon = new Polygon();
            for (int i = 0; i < record.NumberOfParts; i++)
            {
                // Determine the starting index and the end index
                // into the points array that defines the figure.
                int start = record.Parts[i];
                int end;
                if (record.NumberOfParts > 1 && i != (record.NumberOfParts - 1))
                    end = record.Parts[i + 1];
                else
                    end = record.NumberOfPoints;

                ESRI.ArcGIS.Client.Geometry.PointCollection points = new ESRI.ArcGIS.Client.Geometry.PointCollection();
                // Add line segments to the polyline
                for (int j = start; j < end; j++)
                {
                    System.Windows.Point point = record.Points[j];
                    points.Add(new MapPoint(point.X, point.Y));
                }

                polygon.Rings.Add(points);
            }

            return polygon;
        }

        private static ESRI.ArcGIS.Client.Geometry.Geometry GetPoint(ShapeFileRecord record)
        {
            MapPoint point = new MapPoint();
            point.X = record.Points[0].X;
            point.Y = record.Points[0].Y;
            return point;
        }

 
    }
}