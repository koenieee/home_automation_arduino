using HomeAutomation;
using KMPP;
using SlimDX;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColorChooserGRB
{
    public partial class LedsAmbilight : Form
    {

        //&koen&INSTALL SLIMDX TO USE THIS: http://slimdx.org/download.php, x86 version
        //40 on top
        //10 on both sides

        //USED SOURCE: http://www.codeproject.com/Articles/274461/Very-fast-screen-capture-using-DirectX-in-Csharp
        DxScreenCapture sc = new DxScreenCapture();

        Collection<long> tlPos = new Collection<long>();
        Collection<long> tPos = new Collection<long>();
        Collection<long> trPos = new Collection<long>();
        Collection<long> lPos = new Collection<long>();
        Collection<long> rPos = new Collection<long>();
        Collection<long> blPos = new Collection<long>();
        Collection<long> bPos = new Collection<long>();
        Collection<long> brPos = new Collection<long>();

        const int Bpp = 4;

        private void write_color_stream(List<Color> theCols)
        {
            if (theCols != null)
            {
                foreach (Color col in theCols)
                {
                    byte[] tempCol = new byte[3];
                    tempCol[0] = col.G;
                    tempCol[1] = col.R;
                    tempCol[2] = col.B;
                    Form1.ArduinoHome.Write(tempCol, 0, 3);
                }
            }
        }

        public LedsAmbilight()
        {
            InitializeComponent();

            int o = 20;
            int m = 8;
            int sx = Screen.PrimaryScreen.Bounds.Width - m;
            int sy = Screen.PrimaryScreen.Bounds.Height - m;
            int bx = (sx - m) / 3 + m;
            int by = (sy - m) / 3 + m;
            int bx2 = (sx - m) * 2 / 3 + m;
            int by2 = (sy - m) * 2 / 3 + m;

            long x, y;
            long pos;

            y = m; //8

            for (x = m; x < sx; x += o) //TOP
            {
                pos = (y * Screen.PrimaryScreen.Bounds.Width + x) * Bpp;
                if (x < bx) //x is smaller than bx, 173
                    tlPos.Add(pos); //top left
                else if (x > bx && x < bx2) //between bx and bx2
                    tPos.Add(pos); //top
                else if (x > bx2) //more than bx2
                    trPos.Add(pos); //top right
            }

            y = sy; //1080
            for (x = m; x < sx; x += o) //BOTTOM
            {
                pos = (y * Screen.PrimaryScreen.Bounds.Width + x) * Bpp;
                if (x < bx)
                    blPos.Add(pos); //bottom left
                else if (x > bx && x < bx2)
                    bPos.Add(pos); //bottom
                else if (x > bx2)
                    brPos.Add(pos); //bottom right
            }

            x = m;
            for (y = m + 1; y < sy - 1; y += o) //LEFT
            {
                pos = (y * Screen.PrimaryScreen.Bounds.Width + x) * Bpp;
                if (y < by)
                    tlPos.Add(pos); //topLeft
                else if (y > by && y < by2)
                    lPos.Add(pos); //left
                else if (y > by2)
                    blPos.Add(pos); //bottom left
            }

            x = sx;
            for (y = m + 1; y < sy - 1; y += o) //RIGHT
            {
                pos = (y * Screen.PrimaryScreen.Bounds.Width + x) * Bpp;
                if (y < by)
                    trPos.Add(pos); //top right
                else if (y > by && y < by2)
                    rPos.Add(pos); //right
                else if (y > by2)
                    brPos.Add(pos); //Bottom right
            }

        }

        Color avcs(DataStream gs, Collection<long> positions)
        {
            byte[] bu = new byte[4];
            int r = 0;
            int g = 0;
            int b = 0;
            int i = 0;

            foreach (long pos in positions)
            {
                gs.Position = pos;
                gs.Read(bu, 0, 4);
                r += bu[2];
                g += bu[1];
                b += bu[0];
                i++;
            }

            return Color.FromArgb(r / i, g / i, b / i);
        }

        List<Color> splitToLeds(DataStream gs, Collection<long> input, int howMuch)
        {
            List<Color> theList = new List<Color>();
            int len = input.Count();
            int b;
            int i;
            int step = (len / howMuch)-1; //e.g. 3
            int count = 0;
            for (i = 0; i < len; i ++)
            {
                if (count >= howMuch)
                {
                    break;
                }
                Collection<long> tmp = new Collection<long>();
                for (b = 0; b < step; b++)
                {
                    try
                    {
                        tmp.Add(input[i + b]);
                    } catch(ArgumentOutOfRangeException e){
                        break;
                    }
                    
                }
                i = i+b;

                Color thecolo = avcs(gs, tmp);
                theList.Add(thecolo);
                count++;
            }
            return theList;
        }

        void Calculate()
        {
            Surface s = sc.CaptureScreen();
            DataRectangle dr = s.LockRectangle(LockFlags.None);
            DataStream gs = dr.Data;
            List<Color> theCols = new List<Color>();
            /* TODO::
            
            var topList = new Collection<long>(tlPos.Concat(tPos).Concat(trPos).ToList());
            var leftList = new Collection<long>(tlPos.Concat(lPos).Concat(blPos).ToList());
            var rightList = new Collection<long>(trPos.Concat(rPos).Concat(brPos).ToList());
            List<Color> splittedLedsList = new List<Color>(splitToLeds(gs, leftList, 10).Concat(splitToLeds(gs, topList, 40)).Concat(splitToLeds(gs, rightList, 10)));*/
            //LEFT
            theCols.Add(avcs(gs, tlPos));
            theCols.Add(avcs(gs, lPos));
            theCols.Add(avcs(gs, blPos));//blPos //tPos)

            //TOP 
           theCols.Add(avcs(gs, tPos));
            //BOTTOM
            theCols.Add(avcs(gs, bPos));
            
            //RIGHT
            theCols.Add(avcs(gs, trPos));
            theCols.Add(avcs(gs, rPos));
            theCols.Add(avcs(gs, brPos));
            
            
            //theCols.Reverse();
            
                         List<Color> finalList = new List<Color>();

            foreach (Color col in theCols)
            {
                for (int i = 0; i < 7; i++)
                {
                    finalList.Add(col);
                }
            }
            finalList.Add(theCols[theCols.Count - 1]);
            finalList.Add(theCols[theCols.Count - 1]);
            finalList.Add(theCols[theCols.Count - 1]);


            write_color_stream(finalList);
            s.UnlockRectangle();
            s.Dispose();
            Thread.Sleep(Convert.ToInt32(textBox1.Text));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Calculate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

    }
}
