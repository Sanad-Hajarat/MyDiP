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
    
    // Pixels in image interpretation       Prior   Prior   Prior
    // Neighbours in an image:              Prior   PIXEL   After
    //                                      After   After   After
    public class Extraction 
    {
        public static List<Point> allShapes(Bitmap b) 
        { 
            // Base idea:
            // Extract Verticies of all shapes and check neighboring pixels (ignore thick images for now)
            // According to Count(corners/vertices) assign shape (triangle, circle & quadrilateral)
            // after that find a way to assign not only shape but crop/copy bitmap into new one with the whole shape showing
            // probably save all (x,y) points of vertices, do bm of size (max(x) - min(x), max(y) - min(y))
            // then take image borders from (min(x), min(y), max(x), max(y))

            List<Point> list = new List<Point>();

            // int size = 0;
            int bW = b.Width, bH = b.Height;
            
            BitmapData bmd = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int stride = bmd.Stride;
            int offSet = stride - bW;

            unsafe
            {
                byte* ptr = (byte*)bmd.Scan0.ToPointer();
                for (int y = 0; y < bH; y++, ptr += offSet)
                {
                    for (int x = 0; x < bW; x++, ptr++) 
                    {
                        if (*ptr > 126)
                        {
                            if (*(ptr-stride-1) < 126 && *(ptr-stride) < 126 && *(ptr-stride+1) < 126 && *(ptr-1) < 126)
                            {
                                list.Add(new Point(x, y));
                            }
                        }
                    }
                }
            }
            b.UnlockBits(bmd);

            return list;
        }

    }
}