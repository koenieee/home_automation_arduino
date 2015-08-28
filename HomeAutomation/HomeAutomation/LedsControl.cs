using HomeAutomation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorChooserGRB
{
    public partial class LedsControl : Form
    {



        static bool blinkSleep;
        static bool mus;
        static bool blinkingDownOrUp;
        static bool clicked = false;
        static int intensity = 141; //default start value for intensity

        static String textLPC = "";

        static Color prevColor;

        static ColoRand ts = new ColoRand(); //generate random color
        List<Color> currentColor = new List<Color>();
        static bool cycleColor = false;


        static Color cycleColorOne;
        static Color cycleColorTwo;

        static int startpos = 0;
        static int NUMBER_OF_LEDS = 59;
        static int kitt_indicer = 0;

        byte[] colorBytes = new byte[3];
        BackgroundWorker m_oWorker;

        static bool kittStyle = false;
        static bool randomPressed = false;
        static bool fill_leds = false;
        int filled_leds;


        public LedsControl()
        {
            InitializeComponent();
            Form1.ArduinoHome.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandlerLeds);
            
            trackBar1.MouseDown += (s,
                                    e) =>
            {
                clicked = true;
            };
            trackBar1.MouseUp += (s,
                                    e) =>
            {
                if (!clicked)
                    return;

                clicked = false;
                change_brightness(trackBar1.Value);
            };

            //background worker to do things apart from the GUI
            m_oWorker = new BackgroundWorker();
            m_oWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            m_oWorker.WorkerReportsProgress = false; //useless
            m_oWorker.WorkerSupportsCancellation = true;
        }



        private void change_brightness(int value)
        {
            if (!clicked)
            {
                float normalized_value = ((value - 0f) / (255f - 0f)) * (1f - -1f) + -1f; //generate value between -1 and 1, from range 0 to 255

                List<Color> modiCols = new List<Color>();
                if (currentColor != null)
                {
                    foreach (Color col in currentColor)
                    {
                        modiCols.Add(ColorManipulations.ChangeColorBrightness(col, normalized_value));

                    }
                    // coltmp = modiCols;
                    write_color_stream(modiCols);
                }
            }
        }




        private static void DataReceivedHandlerLeds(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            if (sp.IsOpen)
            {
                string indata = sp.ReadExisting();
                textLPC = indata;

                if (indata.Contains("Select-mode"))
                {

                    Form1.ArduinoHome.Write("l");
                }
                else if (indata.Contains("looping"))
                {
                    Form1.connectionStatus.ForeColor = Color.Green;
                    Action updateLabel = () => Form1.connectionStatus.Text = "CONNECTED";
                    Form1.connectionStatus.Invoke(updateLabel);
                }
            }
        }

        private void write_color_stream(List<Color> theCols)
        {
            // coltmp = theCols; //keep track of current colors in led strip
            if (theCols != null)
            {
                foreach (Color col in theCols)
                {
                    byte[] tempCol = new byte[3];
                    tempCol[0] = col.G;
                    tempCol[1] = col.R;
                    tempCol[2] = col.B;
                    Form1.ArduinoHome.Write(tempCol, 0, 3); //write color stream as GRB
                    //  String text = LPC1114.ReadLine();
                    //   if (text != "") {
                    //     MessageBox.Show("The text was: " + text);
                    //   }

                }
                // while (true) {
                //     if (textLPC.Contains("SUCCESS"))
                //      {
                //      MessageBox.Show("Color applied");
                //      break;
                //    }
                //  }
            }
            //  Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            write_color_stream(currentColor);
        }





        //set single color
        private void button2_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                List<Color> colooren = ColorManipulations.GetGradientColors(colorDialog1.Color, colorDialog1.Color, NUMBER_OF_LEDS + 1);
                currentColor = colooren;

                apply_color.BackColor = colorDialog1.Color;
            }
        }

        //set gradient color
        private void button3_Click(object sender, EventArgs e)
        {

            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color one, two;
                one = colorDialog1.Color;
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    two = colorDialog1.Color;
                    List<Color> colooren = ColorManipulations.GetGradientColors(one, two, NUMBER_OF_LEDS +1);
                    currentColor = colooren;
                    write_color_stream(colooren);
                }
            }
        }


        //set random gradient color
        private void button4_Click(object sender, EventArgs e)
        {
            Color one = ts.RandomColor();
            Color two = ts.RandomColor();
            List<Color> colooren = ColorManipulations.GetGradientColors(one, two, NUMBER_OF_LEDS +1);
            currentColor = colooren;
            write_color_stream(colooren);

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (randomPressed)
            {
                if (m_oWorker.IsBusy)
                {
                    m_oWorker.CancelAsync();
                    randomPressed = false;
                }
            }
            else
            {
                randomPressed = true;
                m_oWorker.RunWorkerAsync(); //start background worker, let the show begin..
            }
            
        }

        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!m_oWorker.CancellationPending)
            {


                // Update the colors.
                List<Color> colors = new List<Color>();

                if (blinkSleep) //blinking up and down
                {
                    if (0 <= intensity && intensity <= 10)
                    {
                        blinkingDownOrUp = true; //blinking up

                    }
                    if (135 <= intensity && intensity <= 145)
                    {
                        blinkingDownOrUp = false; //blinking down
                    }

                    if (blinkingDownOrUp) //blinking up
                    {
                        intensity += 5;
                        change_brightness(intensity);
                    }
                    else
                    {
                        intensity -= 5;
                        change_brightness(intensity);
                    }
                }
                else if (cycleColor) //cycle through specifed colors
                {

                    List<Color> colooren = new List<Color>();

                    int otherLeds = Convert.ToInt32(textBox2.Text);
                    if (startpos + otherLeds == NUMBER_OF_LEDS)
                    {
                        blinkingDownOrUp = false;
                    }
                    if (startpos == 0)
                    {
                        blinkingDownOrUp = true;
                    }

                    if (blinkingDownOrUp)
                    {
                        startpos++;
                    }
                    else
                    {
                        startpos--;
                    }
                    for (int i = 0; i < NUMBER_OF_LEDS; i++)
                    {
                        if (startpos <= i && i < startpos + otherLeds)
                        {
                            colooren.Add(cycleColorOne);
                        }
                        else
                        {
                            colooren.Add(cycleColorTwo);
                        }

                    }
                    write_color_stream(colooren);
                }
                else if (kittStyle) //kitt style display leds
                {
                    if (currentColor != null) {
                        int middle = (NUMBER_OF_LEDS / 2);
                        if (kitt_indicer == 0)
                        {
                            blinkingDownOrUp = true;

                        }
                        if (kitt_indicer == middle)
                        {
                            blinkingDownOrUp = false;
                        }
                        if (blinkingDownOrUp)
                        {
                            kitt_indicer++;
                        }
                        else
                        {
                            kitt_indicer--;
                        }
                        List<Color> colooren = new List<Color>();

                        int start = middle - kitt_indicer;
                        int end = middle + kitt_indicer;
                        for (int i = 0; i < NUMBER_OF_LEDS; i++)
                        {
                            if (start <= i && i < end)
                            {
                                colooren.Add(currentColor[0]);
                            }
                            else
                            {
                                colooren.Add(Color.FromArgb(0, 0, 0));
                            }

                        }
                        write_color_stream(colooren);
                    }
                }
                else if (fill_leds)
                {
                    if (currentColor != null)
                    {
                        List<Color> colooren = new List<Color>();
                        if (filled_leds == NUMBER_OF_LEDS)
                        {
                            fill_leds = false;
                            m_oWorker.CancelAsync();
                            break;
                        }
                        if ((NUMBER_OF_LEDS - filled_leds) == startpos)
                        {
                            filled_leds++;
                            startpos = 0;
                        }
                        for (int i = 0; i < NUMBER_OF_LEDS; i++)
                        {
                            if (i >= (NUMBER_OF_LEDS - filled_leds))
                            {
                                colooren.Add(currentColor[0]);
                            }
                            else if (startpos == i)
                            {
                                colooren.Add(currentColor[0]);
                            }
                            else
                            {
                                colooren.Add(Color.FromArgb(0, 0, 0));
                            }

                        }

                        startpos++;
                        write_color_stream(colooren);

                    }
                }
                else //start random color show
                {
                    Random _r = new Random();
                    for (int i = 0; i < NUMBER_OF_LEDS; i++)
                    {
                        colors.Add(Color.FromArgb(_r.Next(0, 255), _r.Next(0, 255), _r.Next(0, 255)));
                    }


                    //     Color one = ts.RandomColor();
                    //     Color two = ts.RandomColor();
                    //     List<Color> colooren = GetGradientColors(one, two, NUMBER_OF_LEDS + 1);
                    write_color_stream(colors);
                }

                Thread.Sleep(Convert.ToInt32(textBox1.Text));
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (kittStyle)
            {
                if (m_oWorker.IsBusy)
                {
                    m_oWorker.CancelAsync();
                    kittStyle = false;
                }
            }
            else
            {
                kittStyle = true;
                m_oWorker.RunWorkerAsync(); //start background worker, let the show begin..
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<Color> colooren = ColorManipulations.GetGradientColors(Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 0), NUMBER_OF_LEDS +1);
            write_color_stream(colooren);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Point cursor = new Point();
            ColorManipulations.GetCursorPos(ref cursor);

            Color c = ColorManipulations.GetColorAt(cursor);

            this.BackColor = c;
            if (c != prevColor)
            {
                prevColor = c;
                List<Color> colooren = ColorManipulations.GetGradientColors(c, c, NUMBER_OF_LEDS +1);
                write_color_stream(colooren);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!mus)
            {
                timer1.Start();
                mus = true;
            }
            else
            {
                timer1.Stop();
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (blinkSleep == false)
            {
                blinkSleep = true;
                if (!m_oWorker.IsBusy)
                {
                    m_oWorker.RunWorkerAsync();
                }
            }
            else
            {
                if (m_oWorker.IsBusy)
                {
                    m_oWorker.CancelAsync();
                    blinkSleep = false;
                }
            }

        }
        //allow only numbers
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        //allow only numbers
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (cycleColor == false)
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    cycleColorOne = colorDialog1.Color;
                    if (colorDialog1.ShowDialog() == DialogResult.OK)
                    {
                        cycleColorTwo = colorDialog1.Color;
                        cycleColor = true;
                        m_oWorker.RunWorkerAsync();

                    }
                }
            }
            else
            {
                if (m_oWorker.IsBusy)
                {
                    m_oWorker.CancelAsync();
                    cycleColor = false;
                }
            }
        }

        //Open form2, and enable AmbiLight possibility
        private void button13_Click(object sender, EventArgs e)
        {
            LedsAmbilight ambilight = new LedsAmbilight();
            ambilight.Show(); //show form
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (fill_leds == false)
            {
                fill_leds = true;
                m_oWorker.RunWorkerAsync();
            }
            else
            {
                if (m_oWorker.IsBusy)
                {
                    fill_leds = false;
                    m_oWorker.CancelAsync();
                }
            }
        }

        private void LedsControl_Load(object sender, EventArgs e)
        {
            Form1.ArduinoHome.Open();
        }

        private void LedsControl_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form1.ArduinoHome.Close();
            Form1.ArduinoHome.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandlerLeds);
            Form1.connectionStatus.ForeColor = Color.Red;
            Action updateLabel = () => Form1.connectionStatus.Text = "Waiting for Connection";
            Form1.connectionStatus.Invoke(updateLabel);
        }
    }
}

