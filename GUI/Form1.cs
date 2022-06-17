using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheWarofThreads;

namespace GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            game = new Game(this);
            InitializeComponent();
        }

        private Game game;

        public Bitmap Bitmap
        {
            get => _bitmap;
            set
            {
                var bmp2 = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = Graphics.FromImage(bmp2))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(value, new Rectangle(Point.Empty, bmp2.Size));
                    pictureBox.Image = bmp2;
                }

                _bitmap = value;
            }
        }

        private Bitmap _bitmap;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }
    }
}