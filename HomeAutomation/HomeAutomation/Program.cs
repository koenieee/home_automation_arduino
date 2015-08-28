using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeAutomation
{
    static class Program
    {
        private static SerialPort curtainSunset;
        private static Boolean connected = false;
        private static Timer timer1 = new Timer();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {

                DialogResult result = MessageBox.Show("Are you sure you want to lower the curtain? (curtain also at full top?)", "Confirmation", MessageBoxButtons.YesNoCancel);
                if(result == DialogResult.Yes)
                {

                    curtainSunset = new SerialPort();
                    curtainSunset.PortName = "COM5";
                    curtainSunset.BaudRate = 115200;
                    curtainSunset.DtrEnable = true;
                    curtainSunset.DataReceived += new SerialDataReceivedEventHandler(doSomethingSerial);

                    curtainSunset.Open();

                    while (!connected) { }
                    curtainSunset.Write("s");
                    timer1.Interval = 9500;
                    timer1.Tick += new EventHandler(_timer_Elapsed);
                    timer1.Enabled = true;
                    timer1.Start();
                }
                else if (result == DialogResult.No)
                {
                    Application.Exit();
                }

            }
        }

        private static void _timer_Elapsed(object sender, EventArgs e)
        {
            curtainSunset.Write("s");
            timer1.Stop();
            curtainSunset.Close();
            Application.Exit();
        }

        private static void doSomethingSerial(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen)
            {
                string indata = sp.ReadExisting();

                if (indata.Contains("Select-mode"))
                {

                    curtainSunset.Write("g");
                }
                else if (indata.Contains("looping"))
                {
                    connected = true;
                }
            }
        }
    }
}
