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
            int filterSize = (int)Math.Sqrt(strel.Length);  // used in traversing strel
            int borderWidth = ((filterSize - 1) / 2);
            int bitmapWidth = b.Width, bitmapHeight = b.Height;

            Bitmap newB = ImageAlteration.AddBorder(b, 0, borderWidth);         // adding black border to apply erosion
            BitmapData bmd1 = newB.LockBits(new Rectangle(0, 0, bitmapWidth + (borderWidth*2), bitmapHeight + (borderWidth*2)),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            Bitmap final = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format8bppIndexed);
            BitmapData bmd2 = final.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            
            int stride1 = bmd1.Stride;
            int offSet1 = stride1 - bitmapWidth, offSet2 = bmd2.Stride - bitmapWidth;
            
            unsafe
            {
                byte* borderedImage = (byte*)bmd1.Scan0.ToPointer();            // original image pixel
                byte* pixel = (byte*)bmd2.Scan0.ToPointer();                    // eroded image pixel

                for (int y = 0; y < bitmapHeight; y++, borderedImage+=offSet1, pixel+=offSet2)
                {
                    for (int x = 0; x < bitmapWidth; x++, pixel++, borderedImage++) // so many if loops to terminate the function with least comparisons possible.
                    {
                        *pixel = 0;
                        for (int i = 0; i < filterSize; i++)
                        {
                            for (int j = 0; j < filterSize; j++)
                            {
                                if (strel[i, j] > 0.5 && *(borderedImage + (stride1*i) + j) > 128)
                                {
                                    *pixel = 255;
                                    break;
                                }
                            }
                            if (*pixel == 255) { break; }
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
            int filterSize = (int)Math.Sqrt(strel.Length);  // used in traversing strel
            int borderWidth = ((filterSize - 1) / 2);       // used in bmp and bmd
            int bitmapWidth = b.Width, bitmapHeight = b.Height;

            Bitmap newB = ImageAlteration.AddBorder(b, 255, borderWidth);       // adding white border to apply erosion
            BitmapData bmd1 = newB.LockBits(new Rectangle(0, 0, bitmapWidth + (borderWidth*2), bitmapHeight + (borderWidth*2)),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            
            Bitmap final = new Bitmap(bitmapWidth, bitmapHeight, PixelFormat.Format8bppIndexed);
            BitmapData bmd2 = final.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            
            int stride1 = bmd1.Stride;
            int offSet1 = stride1 - bitmapWidth, offSet2 = bmd2.Stride - bitmapWidth;
            
            unsafe
            {
                byte* borderedImage = (byte*)bmd1.Scan0.ToPointer();            // original image pixel
                byte* pixel = (byte*)bmd2.Scan0.ToPointer();                    // dilated image pixel

                for (int y = 0; y < bitmapHeight; y++, borderedImage+=offSet1, pixel+=offSet2)
                {
                    for (int x = 0; x < bitmapWidth; x++, pixel++, borderedImage++) // so many if loops to terminate the function with least comparisons possible.
                    { 
                        *pixel = 255;
                        for (int i = 0; i < filterSize; i++)
                        {
                            for (int j = 0; j < filterSize; j++)
                            {
                                if (strel[i, j] < 0.5 && *(borderedImage + (stride1*i) + j) < 128)
                                {
                                    *pixel = 0;
                                    break;
                                }
                            }
                            if (*pixel == 0) { break; }
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