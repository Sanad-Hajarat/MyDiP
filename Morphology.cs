using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    public class Morphology
    {
        private static int[,] sqr = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };   // base structuring element

        public static Bitmap Erode(Bitmap b) { return Erode(b, sqr); }
        public static Bitmap Dilate(Bitmap b) { return Dilate(b, sqr); }

        public static Bitmap Erode(Bitmap b, int[,] strel)
        {
            Bitmap newB = ImageAlteration.AddBorder(b, 0);                      // adding black border to apply erosion
            int bW = b.Width, bH = b.Height, bW2 = bW+2, bH2 = bH+2;
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            BitmapData bmd1 = newB.LockBits(new Rectangle(0, 0, bW2, bH2), ImageLockMode.ReadOnly, pF);
            Bitmap final = new Bitmap(bW, bH, pF);
            BitmapData bmd2 = final.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, pF);
            int t1 = strel[0, 0], t2 = strel[0, 1], t3 = strel[0, 2],           // t signifies top    3 pixels
                m1 = strel[1, 0], m2 = strel[1, 1], m3 = strel[1, 2],           // m signifies middle 3 pixels
                b1 = strel[2, 0], b2 = strel[2, 1], b3 = strel[2, 2];           // b signifies bottom 3 pixels
            
            int stride1 = bmd1.Stride, stride2 = bmd2.Stride;
            int offSet1 = stride1 - bW, offSet2 = stride2 - bW;
            
            unsafe
            {
                byte* top = (byte*)bmd1.Scan0.ToPointer();                      // first ptr for first row of pixels
                byte* mid = top + stride1;                                      // second ptr for first second of pixels
                byte* bot = mid + stride1;                                      // third ptr for third row of pixels
                byte* pixel = (byte*)bmd2.Scan0.ToPointer();                    // eroded image pixel

                for (int y = 0; y < bH; y++, top+=offSet1, mid+=offSet1, bot+=offSet1, pixel+=offSet2)
                {
                    for (int x = 0; x < bW; x++, pixel++, top++, mid++, bot++) // so many if loops to terminate the function with least comparisons possible.
                    {
                        if (t1 > 0.5 && top[0] > 128) 
                            pixel[0] = 255;
                        else
                        {
                            if (t2 > 0.5 && (top + 1)[0] > 128)
                                pixel[0] = 255;
                            else
                            {
                                if (t3 > 0.5 && (top + 2)[0] > 128)
                                    pixel[0] = 255;
                                else
                                {
                                    if (m1 > 0.5 && mid[0] > 128)
                                        pixel[0] = 255;
                                    else
                                    {
                                        if (m2 > 0.5 && (mid + 1)[0] > 128)
                                            pixel[0] = 255;
                                        else
                                        {
                                            if (m3 > 0.5 && (mid + 2)[0] > 128)
                                                pixel[0] = 255;
                                            else
                                            {
                                                if (b1 > 0.5 && bot[0] > 128)
                                                    pixel[0] = 255;
                                                else
                                                {
                                                    if (b2 > 0.5 && (bot + 1)[0] > 128)
                                                        pixel[0] = 255;
                                                    else
                                                    {
                                                        if (b3 > 0.5 && (bot + 2)[0] > 128)
                                                            pixel[0] = 255;
                                                        else
                                                            pixel[0] = 0;   // not successful if's means its a black pixel
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            newB.UnlockBits(bmd1);
            final.UnlockBits(bmd2);
            

            return final;
        }
        public static Bitmap Dilate(Bitmap b, int[,] strel)
        {
            Bitmap newB = ImageAlteration.AddBorder(b, 255);                    // adding black border to apply erosion
            int bW = b.Width, bH = b.Height, bW2 = bW+2, bH2 = bH+2;
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            BitmapData bmd1 = newB.LockBits(new Rectangle(0, 0, bW2, bH2), ImageLockMode.ReadOnly, pF);
            
            Bitmap final = new Bitmap(bW, bH, pF);
            BitmapData bmd2 = final.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, pF);
            
            int t1 = strel[0, 0], t2 = strel[0, 1], t3 = strel[0, 2],           // t signifies top    3 pixels
                m1 = strel[1, 0], m2 = strel[1, 1], m3 = strel[1, 2],           // m signifies middle 3 pixels
                b1 = strel[2, 0], b2 = strel[2, 1], b3 = strel[2, 2];           // b signifies bottom 3 pixels
            
            int stride1 = bmd1.Stride, stride2 = bmd2.Stride;
            int offSet1 = stride1 - bW, offSet2 = stride2 - bW;
            
            unsafe
            {
                byte* top = (byte*)bmd1.Scan0.ToPointer();                      // first ptr for first row of pixels
                byte* mid = top + stride1;                                      // second ptr for second row of pixels
                byte* bot = mid + stride1;                                      // third ptr for third row of pixels
                byte* pixel = (byte*)bmd2.Scan0.ToPointer();                    // dilated image pixel

                for (int y = 0; y < bH; y++, top+=offSet1, mid+=offSet1, bot+=offSet1, pixel+=offSet2)
                {
                    for (int x = 0; x < bW; x++, pixel++, top++, mid++, bot++) // so many if loops to terminate the function with least comparisons possible.
                    { 
                        if (t1 < 0.5 && top[0] < 128)
                            pixel[0] = 0;
                        else
                        {
                            if (t2 < 0.5 && (top + 1)[0] < 128)
                                pixel[0] = 0;
                            else
                            {
                                if (t3 < 0.5 && (top + 2)[0] < 128)
                                    pixel[0] = 0;
                                else
                                {
                                    if (m1 < 0.5 && mid[0] < 128)
                                        pixel[0] = 0;
                                    else
                                    {
                                        if (m2 < 0.5 && (mid + 1)[0] < 128)
                                            pixel[0] = 0;
                                        else
                                        {
                                            if (m3 < 0.5 && (mid + 2)[0] < 128)
                                                pixel[0] = 0;
                                            else
                                            {
                                                if (b1 < 0.5 && bot[0] < 128)
                                                    pixel[0] = 0;
                                                else
                                                {
                                                    if (b2 < 0.5 && (bot + 1)[0] < 128)
                                                        pixel[0] = 0;
                                                    else
                                                    {
                                                        if (b3 < 0.5 && (bot + 2)[0] < 128)
                                                            pixel[0] = 0;
                                                        else
                                                            pixel[0] = 255; // not successful if's means its a white pixel
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            newB.UnlockBits(bmd1);
            final.UnlockBits(bmd2); 
            
            return final;
        }
        
    }
}