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
            InitializeComponent();
            pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
            using (var g = pictureBox.CreateGraphics())
                g.Clear(Color.Black);
            game = new Game(this);
        }

        private Game game;


        public void SetImage(Bitmap value)
        {
            try
            {
                //pictureBox.Image = null;
                // var bmp2 = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = pictureBox.CreateGraphics())
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(value, new Rectangle(Point.Empty, pictureBox.Image.Size));
                    // pictureBox.Image?.Dispose();
                    // pictureBox.Image = bmp2;
                }
            }
            catch
            {
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            ConsoleKey? key = null;
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    //key = ConsoleKey.Enter;
                    break;
                case Keys.Left:
                    key = ConsoleKey.LeftArrow;
                    break;
                case Keys.Right:
                    key = ConsoleKey.RightArrow;
                    break;
                case Keys.Space:
                    key = ConsoleKey.Spacebar;
                    break;
                default:
                    break;
            }

            if (key != null)
                game.Console.Keys.Enqueue(key.Value);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            game.mainThread.Abort();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
                using (var g = pictureBox.CreateGraphics())
                    g.Clear(Color.Black);
            }
            catch
            {
            }
        }
    }
}