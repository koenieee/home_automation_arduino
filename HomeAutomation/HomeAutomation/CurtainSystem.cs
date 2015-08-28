using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TaskScheduler;


namespace HomeAutomation
{
    public partial class CurtainSystem : Form
    {

        public string position = "down";
        public int positionInPercentage;

        public DateTime sunset;

        public CurtainSystem()
        {
            InitializeComponent();




            //Setup data binding

            Form1.ArduinoHome.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerCurt);

            trackBar1.MouseUp += (s,
                                    e) =>
            {
                goToPercentage(trackBar1.Value);
            };
        }

        private void write_button(string theButton)
        {
            if (theButton != null)
            {
                Form1.ArduinoHome.WriteLine(theButton);
            }
        }

        private void DataReceivedHandlerCurt(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen) {
                string indata = sp.ReadExisting();

                if (indata.Contains("Select-mode"))
                {

                    Form1.ArduinoHome.Write("g");
                }
                else if (indata.Contains("looping"))
                {
                    Form1.connectionStatus.ForeColor = Color.Green;
                    Action updateLabel = () => Form1.connectionStatus.Text = "CONNECTED";
                    Form1.connectionStatus.Invoke(updateLabel);
                }
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            write_button("w");
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            write_button("d");
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            write_button("s");
        }

        //to down
        private void button4_Click(object sender, EventArgs e)
        {
            position = "going down";
            write_button("s");
            timer1.Interval = 9500;
            timer1.Tick += new EventHandler(_timer_Elapsed);
            timer1.Enabled = true;
            timer1.Start();
        }

        private void _timer_Elapsed(object sender, EventArgs e)
        {
            positionInPercentage = trackBar1.Value;
            write_button("d");
            timer1.Stop();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            position = "going up";
            write_button("w");
            timer1.Interval = 12000;
            timer1.Tick += new EventHandler(_timer_Elapsed);
            timer1.Enabled = true;
            timer1.Start();
        }

        public void goToPercentage(int per)
        {
            //to go up, 12000 == 100;
            if (positionInPercentage < per) //so we go up
            {
                int percentage = per - positionInPercentage;
                int timeValue = 12000 * percentage / 100; //- (12000 * positionInPercentage / 100);
                position = "custom";
                timer1.Interval = timeValue;
                timer1.Tick += new EventHandler(_timer_Elapsed);
                timer1.Enabled = true;
                timer1.Start();
                write_button("w");
            }
            else //now we go down
            {
                int percentage = positionInPercentage - per;
                int timeValue = 9000 * percentage / 100; //- (12000 * positionInPercentage / 100);
                position = "custom";
                timer1.Interval = timeValue;
                timer1.Tick += new EventHandler(_timer_Elapsed);
                timer1.Enabled = true;
                timer1.Start();
                write_button("s");
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            goToPercentage(trackBar1.Value);
        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Is the curtan at the moment at the full bottom or the full top? (bottom=yes, top=no)", "Calibrate curtain", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                trackBar1.Value = 0;
                positionInPercentage = 0;
            }
            else
            {
                trackBar1.Value = 100;
                positionInPercentage = 100;
            }
        }

        private void CurtainSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ArduinoHome.Close();
            Form1.ArduinoHome.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandlerCurt);
            Form1.connectionStatus.ForeColor = Color.Red;
            Action updateLabel = () => Form1.connectionStatus.Text = "Waiting for Connection...";
            Form1.connectionStatus.Invoke(updateLabel);
        }

        public class Sunset
        {

            [JsonProperty(PropertyName = "sunrise")]
            public string sunrise { get; set; }

            [JsonProperty(PropertyName = "sunset")]
            public string sunset { get; set; }

        }


        private void CurtainSystem_Load(object sender, EventArgs e)
        {

            WebRequest request = WebRequest.Create("http://api.sunrise-sunset.org/json?lat=52.0855972&lng=5.305249,17&formatted=0");
            request.Credentials = CredentialCache.DefaultCredentials;

            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();

            dynamic m = JsonConvert.DeserializeObject(responseFromServer);

            DateTime name = m.results.sunset;
            sunset = name;
            label1.Text = "Sunset at: " + name.TimeOfDay;// myDate1.ToString();




            Form1.ArduinoHome.Open();
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

    }
}
