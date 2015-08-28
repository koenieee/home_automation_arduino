using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorChooserGRB
{
    public partial class ColoRand : Component
    {
        Random _r = null;
        public ColoRand()
        {
            _r = new Random();
        }

        public Color RandomColor()
        {
            return Color.FromArgb(_r.Next(0, 255), _r.Next(0, 255), _r.Next(0, 255));
        }
    }
}
