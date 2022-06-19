using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheWarofThreads;

namespace GUI
{
    internal static class Program
    {
        // public static void SetAlpha(this Bitmap bmp, byte alpha)
        // {
        //     if (bmp == null) throw new ArgumentNullException("bmp");
        //
        //     var data = bmp.LockBits(
        //         new Rectangle(0, 0, bmp.Width, bmp.Height),
        //         System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //         System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        //
        //     var line = data.Scan0;
        //     var eof = line + data.Height * data.Stride;
        //     while (line != eof)
        //     {
        //         var pixelAlpha = line + 3;
        //         var eol = pixelAlpha + data.Width * 4;
        //         while (pixelAlpha != eol)
        //         {
        //             System.Runtime.InteropServices.Marshal.WriteByte(
        //                 pixelAlpha, alpha);
        //             pixelAlpha += 4;
        //         }
        //
        //         line += data.Stride;
        //     }
        //
        //     bmp.UnlockBits(data);
        // }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Game.ConsoleClass.PngBullet = new Bitmap("bullet_s.png");
            // Game.ConsoleClass.PngBullet.SetAlpha(255);
            Game.ConsoleClass.PngInvader = new Bitmap("space_invaders_s.png");
            // Game.ConsoleClass.PngInvader.SetAlpha(255);
            Game.ConsoleClass.PngShip = new Bitmap("space_ship_s.png");
            // Game.ConsoleClass.PngShip.SetAlpha(255);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}