using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace MVMApp
{
    public partial class Form1 : Form
    {
        string URL = "http://192.168.1.52/";
        public class PressureReading
        {
            public double pressure { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string hardware { get; set; }
            public bool connected { get; set; }
        }


        public PlotModel myModel;
        LineSeries ss;
        PlotView myPlot;
        PlotModel model;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myPlot = new PlotView();

            //Create Plotmodel object
            model = new PlotModel() { LegendSymbolLength = 24 };
             ss = new LineSeries()
            {
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 2,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5
            };

            


            model.Series.Add(ss);
            //Assign PlotModel to PlotView
            myPlot.Model = model;

            //Set up plot for display
            myPlot.Dock = System.Windows.Forms.DockStyle.Bottom;
            myPlot.Location = new System.Drawing.Point(0, 0);
            myPlot.Size = new System.Drawing.Size(500, 500);
            myPlot.TabIndex = 0;
            myPlot.Dock = DockStyle.Fill;
            //Add plot control to form
            panel1.Controls.Add(myPlot);

            timer1.Enabled = true;
        }
        int ttt=0;
        double mmin, mmax;

        private void bRun_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            System.Threading.Thread.Sleep(500);
            if (bRun.Tag == "RUN")
            {
                bRun.Text = "RUN";
                bRun.Tag = "STOP";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL + "run?params=0");
                request.ContentType = "application/json; charset=utf-8";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
            }
            else
            {
                bRun.Text = "STOP";
                bRun.Tag = "RUN";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL + "run?params=1");
                request.ContentType = "application/json; charset=utf-8";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                response.Close();
            }
            timer1.Enabled = true;
        }

        private void set_param(String Param, String Value)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL + Param+ "?params=" + Value);
            request.Timeout = 1000;
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            response.Close();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            System.Threading.Thread.Sleep(500);
            set_param("inhale_ms", numericUpDown1.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("exhale_ms", numericUpDown2.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("inhale_alarm_ms", numericUpDown7.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("exhale_alarm_ms", numericUpDown6.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("pressure_max", numericUpDown3.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("pressure_min", numericUpDown4.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            set_param("pressure_drop", numericUpDown5.Value.ToString().Replace(",", "."));
            System.Threading.Thread.Sleep(100);
            MessageBox.Show("Done");
            timer1.Enabled = true;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL + "pressure");
            request.Timeout = 300;
            request.ContentType = "application/json; charset=utf-8";
            try
            {

            
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                String data = reader.ReadToEnd();
                    response.Close();
                Console.WriteLine(data);
                try
                {
                    PressureReading psr = JsonConvert.DeserializeObject<PressureReading>(data);

                    mmin = Math.Min((double)mmin, (double)psr.pressure);
                    mmax = Math.Max((double)mmax, (double)psr.pressure);
                    model.DefaultYAxis.Zoom(mmin-0.05* Math.Abs(mmin), mmax+0.05* Math.Abs(mmax));

                    ss.Points.Add(new DataPoint(ttt, psr.pressure));
                    ttt++;

                    if (ttt > 90)
                    {
                        double panStep = model.DefaultXAxis.Transform(-1 + model.DefaultXAxis.Offset);
                        model.DefaultXAxis.Pan(panStep);
                    }
                    myPlot.Refresh();
          

                } catch (Exception ex)
                {
                

                }
                
            }
            }
            catch (Exception ex)
            {

                
            }





        }
    }
}
