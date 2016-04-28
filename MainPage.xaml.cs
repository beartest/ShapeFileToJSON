using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using System.Json;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using geometry_json_translate.MyServiceReference1;

namespace geometry_json_translate
{
    public partial class MainPage : UserControl
    {
        ShapeFile shapeFileReader = new ShapeFile();
        string names = null;

        public MainPage()
        {
            InitializeComponent();
        }

        #region      按钮变色
        private void textBlock1_MouseEnter(object sender, MouseEventArgs e)
        {
            rectangle4.Fill = new SolidColorBrush(Color.FromArgb(119, 185, 185, 185));
            textBlock1.Foreground = new SolidColorBrush(Colors.White);
        }

        private void rectangle4_MouseEnter(object sender, MouseEventArgs e)
        {
            rectangle4.Fill = new SolidColorBrush(Color.FromArgb(119, 185, 185, 185));
            textBlock1.Foreground = new SolidColorBrush(Colors.White);
        }

        private void rectangle4_MouseLeave(object sender, MouseEventArgs e)
        {
            rectangle4.Fill = new SolidColorBrush(Color.FromArgb(119, 145, 145, 145));
            textBlock1.Foreground = new SolidColorBrush(Color.FromArgb(225, 220, 220, 220));
        }
        #endregion

        private void MyClient1_DynamicAccessCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("成功生成表");
        }

        private void MyClient2_MyInsertCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
        }

        //打开
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text == null)
            {
                MessageBox.Show("数据库名不能为空");
                return;
            }
            if (textBox2.Text == null)
            {
                MessageBox.Show("表名不能为空");
                return;
            }
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.InitialDirectory = "E:\\0cuijing\\geometry-json-translate-属性保存\\shapefile";
            dialog.Filter = "Shape Files (*.shp;*.dbf)|*.shp;*.dbf";
            dialog.ShowDialog();
            if (dialog.Files == null)
                return;
            //判断拖放的文件是否为.shp和.dbf
            FileInfo shapefile = null;
            FileInfo dbffile = null;
            foreach (FileInfo fi in dialog.Files)
            {
                if (fi.Extension.ToLower() == ".shp")
                    shapefile = fi;
                if (fi.Extension.ToLower() == ".dbf")
                    dbffile = fi;
            }
            // 读取Shapefile数据  
            if (shapefile != null && dbffile != null)
            {
                shapeFileReader.Read(shapefile, dbffile);
            }
            else
            {
                MessageBox.Show("请同时选择.dbf和.shp文件");
                return;
            }
        }
        //建立表
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            ShapeFileRecord record1 = shapeFileReader.Records[0];
            Graphic g1 = record1.ToGraphic();
            if (g1 != null)
            {
                FeatureSet fs1 = new FeatureSet();
                fs1.Features.Add(g1);
                string json1 = fs1.ToJson();
                JsonObject jsonObj1 = JsonObject.Parse(json1) as JsonObject;
                if (jsonObj1.ContainsKey("features"))
                {
                    JsonValue jsonFea1 = jsonObj1["features"];
                    if (jsonFea1 is JsonArray)
                    {
                        if (jsonFea1[0].ContainsKey("attributes"))
                        {
                            JsonValue jsonAtt1 = jsonFea1[0]["attributes"];
                            JsonObject jsonAO1 = jsonAtt1 as JsonObject;
                            ArrayOfString Names = new ArrayOfString();
                            ArrayOfString Types = new ArrayOfString();
                            //获取名
                            foreach (string name in jsonAO1.Keys)
                            {
                                Names.Add(name);
                                names += "," + name;
                            }
                            Names.Add("Json");
                            names += ",Json";
                            //获取属性类型
                            foreach (JsonValue item in jsonAO1.Values)
                            {
                                switch (item.JsonType)
                                {
                                    case JsonType.String:
                                        Types.Add((typeof(String)).ToString());
                                        break;
                                    case JsonType.Number:
                                        Types.Add((typeof(Double)).ToString());
                                        break;
                                    case JsonType.Boolean:
                                        Types.Add((typeof(Boolean)).ToString());
                                        break;
                                    case JsonType.Array:
                                        Types.Add((typeof(Array)).ToString());
                                        break;
                                    case JsonType.Object:
                                        Types.Add((typeof(Object)).ToString());
                                        break;
                                    default: MessageBox.Show("未识别类型");
                                        break;
                                }
                            }
                            Types.Add("Json");
                            MyWebService1SoapClient MyClient1 = new MyWebService1SoapClient();
                            MyClient1.DynamicAccessCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MyClient1_DynamicAccessCompleted);
                            MyClient1.DynamicAccessAsync(textBox1.Text + ".mdb", textBox2.Text, Names, Types);
                        }
                    }
                }
            }
        }
        //保存属性
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Graphic g = new Graphic();
            foreach (ShapeFileRecord record in shapeFileReader.Records)
            {
                g = record.ToGraphic();
                if (g != null)
                {
                    FeatureSet fs = new FeatureSet();
                    fs.Features.Add(g);
                    string json = fs.ToJson();
                    JsonObject jsonObj = JsonObject.Parse(json) as JsonObject;
                    if (jsonObj.ContainsKey("features"))
                    {
                        JsonValue jsonFea = jsonObj["features"];
                        if (jsonFea is JsonArray)
                        {
                            for (int i = 0; i < jsonFea.Count; i++)
                            {
                                if (jsonFea[i].ContainsKey("attributes"))
                                {
                                    JsonValue jsonAtt = jsonFea[i]["attributes"];
                                    JsonObject jsonAO = jsonAtt as JsonObject;
                                    string values = "";
                                    foreach (JsonValue item in jsonAO.Values)
                                    {
                                        string s = item.ToString();
                                        s = s.Substring(0, s.Length - 1);
                                        s = s.Substring(1);
                                        switch (item.JsonType)
                                        {
                                            case JsonType.String:
                                                values += ",'" + s.Trim() + "'";
                                                break;
                                            case JsonType.Number:
                                            case JsonType.Boolean:
                                            case JsonType.Array:
                                            case JsonType.Object:
                                                values += "," + s.Trim();
                                                break;
                                        }
                                    }
                                    values += ",'" + json + "'";
                                    MyWebService1SoapClient MyClient2 = new MyWebService1SoapClient();
                                    MyClient2.MyInsertCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(MyClient2_MyInsertCompleted);
                                    MyClient2.MyInsertAsync(names, values, textBox1.Text + ".mdb", textBox2.Text);
                                }
                            }
                        }
                    }
                }
            }
        }

        
    }
}
