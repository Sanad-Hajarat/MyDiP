using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic; 

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>

    public class Shapes
    {
        // public static int CountCor(Bitmap[] arrB) 
        // { 
        //     int circle = 1, triangle = 1, square = 1, rectangle = 1;
        //     int corners;
        //     double aspectRatio;

        //     foreach (Bitmap b in arrB)
        //     {
        //         corners = 0;

        //         int bW = b.Width, bH = b.Height;

        //         aspectRatio = bW/bH;

        //         BitmapData bmd = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

        //         int offSet = bmd.Stride - bW;

        //         unsafe
        //         {
        //             byte* ptr = (byte*)bmd.Scan0.ToPointer();
        //             // for (int y = 0; y < bH; y++, ptr += offSet)
        //             // {
        //             //     for (int x = 0; x < bW; x++, ptr++) 
        //             //     {
        //             //         if (*(ptr-1) == 0 && *ptr == 0)  
        //             //     }
        //             // }
        //         }

        //         b.UnlockBits(bmd);

        //         if (corners == 3)
        //             b.Save($"Triangle{triangle++}.jpg", ImageFormat.Jpeg);
        //         else if (corners == 4 && aspectRatio >= 0.9 && aspectRatio <= 1.1)
        //             b.Save($"Square{square++}.jpg", ImageFormat.Jpeg);
        //         else if (corners == 4)
        //             b.Save($"Rectangle{rectangle++}.jpg", ImageFormat.Jpeg);
        //         else
        //             b.Save($"Circle{circle++}.jpg", ImageFormat.Jpeg);
        //     }
        //     return corners;

        // }

        public static int CountCorners(List<Point> contour, double thresholdAngle = 0.3)
        {
            int corners = 0;
            int count = contour.Count;

            for (int i = 1; i < count - 1; i++)
            {
                Point prev = contour[i - 1];
                Point current = contour[i];
                Point next = contour[i + 1];

                double angle = Math.Abs(Math.Atan2(next.Y - current.Y, next.X - current.X) - Math.Atan2(prev.Y - current.Y, prev.X - current.X));

                if (angle > Math.PI)
                    angle = 2 * Math.PI - angle;

                if (angle > thresholdAngle)
                    corners++;
            }

            return corners;
        }
        // Base idea:
        // Extract Verticies of all shapes and check neighboring pixels (ignore thick images for now)
        // According to Count(corners/vertices) assign shape (triangle, circle & quadrilateral)
        // after that find a way to assign not only shape but crop/copy bitmap into new one with the whole shape showing
        // probably save all (x,y) points of vertices, do bm of size (max(x) - min(x), max(y) - min(y))
        // then take image borders from (min(x), min(y), max(x), max(y))

    }
}