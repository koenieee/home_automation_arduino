using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HomeAutomation
{
    public partial class AnalogLedControl : Form
    {
         private Rectangle rect;
        private Pen pen = Pens.Black;


        private List<string> IR_CODES = new List<string>(
            new string[] {
            "FF3AC5","FFBA45","FF827D","FF02FD",
            "FF1AE5","FF9A65","FFA25D","FF22DD",
            "FF2AD5","FFAA55","FF926D","FF12ED",
            "FF0AF5","FF8A75","FFB24D","FF32CD",

            "FF38C7","FFB847","FF7887","FFF807",
            "FF18E7","FF9867","FF58A7","FFD827",
            "FF28D7","FFA857","FF6897","FFE817",
            "FF08F7","FF8877","FF48B7","FFC837",

            "FF30CF","FFB04F","FF708F","FFF00F",
            "FF10EF","FF906F","FF50AF","FFD02F",
            "FF20DF","FFA05F","FF609F","FFE01F",
            }
            
            );

        private List<Rectangle> clickableAreas = new List<Rectangle>(
        new Rectangle[]{

            new Rectangle(45, 45, 30, 30), new Rectangle(117, 45, 30, 30), new Rectangle(190, 45, 30, 30), new Rectangle(264, 45, 30, 30),

            new Rectangle(45, 111, 30, 30), new Rectangle(117, 111, 30, 30), new Rectangle(190, 111, 30, 30), new Rectangle(264, 111, 30, 30),

            new Rectangle(45, 174, 30, 30), new Rectangle(117, 174, 30, 30), new Rectangle(190, 174, 30, 30), new Rectangle(264, 174, 30, 30),

            new Rectangle(45, 237, 30, 30), new Rectangle(117, 237, 30, 30), new Rectangle(190, 237, 30, 30), new Rectangle(264, 237, 30, 30),

            new Rectangle(45, 300, 30, 30), new Rectangle(117, 300, 30, 30), new Rectangle(190, 300, 30, 30), new Rectangle(264, 300, 30, 30),

            new Rectangle(45, 363, 30, 30), new Rectangle(117, 363, 30, 30), new Rectangle(190, 363, 30, 30), new Rectangle(264, 363, 30, 30),

            new Rectangle(45, 425, 30, 30), new Rectangle(117, 425, 30, 30), new Rectangle(190, 425, 30, 30), new Rectangle(264, 425, 30, 30),

            new Rectangle(45, 488, 30, 30), new Rectangle(117, 488, 30, 30), new Rectangle(190, 488, 30, 30), new Rectangle(264, 488, 30, 30),

            new Rectangle(45, 551, 30, 30), new Rectangle(117, 551, 30, 30), new Rectangle(190, 551, 30, 30), new Rectangle(264, 551, 30, 30),

            new Rectangle(45, 614, 30, 30), new Rectangle(117, 614, 30, 30), new Rectangle(190, 614, 30, 30), new Rectangle(264, 614, 30, 30),

            new Rectangle(45, 677, 30, 30), new Rectangle(117, 677, 30, 30), new Rectangle(190, 677, 30, 30), new Rectangle(264, 677, 30, 30),
        
        }
            );

        public AnalogLedControl()
        {
            InitializeComponent();
            Form1.ArduinoHome.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerAn);

      //      rect = new Rectangle(10, 10, 40, 40);
    //        
      //      Click += Form1_Click;

        }

        private void DataReceivedHandlerAn(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen) {
                string indata = sp.ReadExisting();

                if (indata.Contains("Select-mode"))
                {

                    Form1.ArduinoHome.Write("r");
                }
                else if (indata.Contains("looping"))
                {
                    Form1.connectionStatus.ForeColor = Color.Green;
                    Action updateLabel = () => Form1.connectionStatus.Text = "CONNECTED";
                    Form1.connectionStatus.Invoke(updateLabel);
                }
            }
        }


        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            foreach (Rectangle rectus in clickableAreas)
            {
                using (Pen pen = new Pen(Color.Transparent, 0))
                {
                    e.Graphics.DrawRectangle(pen, rectus);
                }
            }
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
          //  MessageBox.Show("The rectangle contains: " + rect.ToString());
            int indexKey = clickableAreas.FindIndex(row => row.Contains(coordinates));
            //MessageBox.Show("The code would be: " + IR_CODES[indexKey]);

            if (indexKey != -1)
            {
                Form1.ArduinoHome.WriteLine(IR_CODES[indexKey]);
            }

        }

        private void AnalogLedControl_Load(object sender, EventArgs e)
        {
            Form1.ArduinoHome.Open();
        }

        private void AnalogLedControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ArduinoHome.Close();
            Form1.ArduinoHome.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandlerAn);
            Form1.connectionStatus.ForeColor = Color.Red;
            Action updateLabel = () => Form1.connectionStatus.Text = "Waiting for Connection...";
            Form1.connectionStatus.Invoke(updateLabel);
        }
    }
}
