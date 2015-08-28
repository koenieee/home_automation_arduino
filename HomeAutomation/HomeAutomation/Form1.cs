using ColorChooserGRB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeAutomation
{
	public partial class Form1 : Form
	{

		public static SerialPort ArduinoHome = new SerialPort();

		public List<int> baudrates = new List<int>(new int[] { 110, 150, 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 });

		public static Label connectionStatus;

        public static Boolean connected = false;
        public static Boolean whatType = false;

		public Form1()
		{
			InitializeComponent();

			ArduinoHome.PortName = "COM5";
			ArduinoHome.BaudRate = 115200;
			ArduinoHome.DtrEnable = true;

			connectionStatus = label3;
			comports.DataSource = SerialPort.GetPortNames();
			baudspeed.DataSource = baudrates;

		}

		private void button1_Click(object sender, EventArgs e)
		{
			CurtainSystem frm = new CurtainSystem();
			frm.Show();
		}

		private void comports_SelectedIndexChanged(object sender, EventArgs e)
		{
			string itemSelected = comports.SelectedItem.ToString();
			Console.WriteLine("You selected: " + itemSelected);

			ArduinoHome.PortName = itemSelected;
		}

		private void baudspeed_SelectedIndexChanged(object sender, EventArgs e)
		{
			string itemSelected = baudspeed.SelectedItem.ToString();
			Console.WriteLine("You selected: " + itemSelected);

			ArduinoHome.BaudRate = Convert.ToInt32(itemSelected);
		}


		private void button2_Click(object sender, EventArgs e)
		{
			LedsControl ljd = new LedsControl();
			ljd.Show();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			AnalogLedControl alc = new AnalogLedControl();
			alc.Show();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			Form1.ArduinoHome.Close();
		}

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.ArduinoHome.DataReceived += new SerialDataReceivedEventHandler(whySoSerial);

            whatType = false;
            Form1.ArduinoHome.Open();
            
            //analog leds
            while (!connected) { }
            Thread.Sleep(100);
            Form1.ArduinoHome.WriteLine("FF02FD");
            Thread.Sleep(100);
            Form1.ArduinoHome.Close();

            connected = false;
            whatType = true;
            Form1.ArduinoHome.Open();
            
            while (!connected) { }
            
            
            byte[] blackBytes = new byte[3];
            blackBytes[0] = 0;
            blackBytes[1] = 0;
            blackBytes[2] = 0;
            for (int i = 0; i < 59; i++)
            {
                Form1.ArduinoHome.Write(blackBytes, 0, 3); //write color stream as GRB
            }
            Form1.ArduinoHome.Close();
            Form1.ArduinoHome.DataReceived -= new SerialDataReceivedEventHandler(whySoSerial);

        }

        private static void whySoSerial(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen)
            {
                string indata = sp.ReadExisting();

                if (indata.Contains("Select-mode"))
                {
                    if (whatType == false)
                    {
                        Form1.ArduinoHome.Write("r");
                    }
                    if (whatType)
                    {
                        Form1.ArduinoHome.Write("l");
                    }
                    
                }
                else if (indata.Contains("looping"))
                {
                    connected = true;
                }
            }
        }


	}
}
