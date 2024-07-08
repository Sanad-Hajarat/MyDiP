using System.Drawing;
using System.Drawing.Imaging;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    public class Binarization //We use static across class because the class is independent of an object, containing only methods
    {
        public static Bitmap ApplyStaticThreshold(Bitmap b, int t) { return ApplyThreshold(b, t); }
        public static Bitmap ApplyMeanThreshold(Bitmap b) { return ApplyThreshold(b, GetMean(b)); }
        private static Bitmap ApplyThreshold(Bitmap b, int t)   // Look for repeated values and try to change in it.
        {
            t = (byte)((t < 0) ? 0 : (t > 255) ? 255 : t);      // Pixel Value is manipulated
            
            int bW = b.Width, bH = b.Height;
            Bitmap copy = (Bitmap)ImageAlteration.GrayScale(b).Clone(); // Use clone with grayscale because it is changing original image
            BitmapData bmd1 = copy.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            int stride = bmd1.Stride;
            int offSet = stride - bW;
            
            unsafe
            { 
                byte* pixel = (byte*)bmd1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap.

               // Loop through the pixels.
               for (int y = 0; y < bH; y++)
               {
                   for (int x = 0; x < bW; x++, pixel++)
                        pixel[0] = (byte)(pixel[0] > t ? 255 : 0); // changes pixel value according to if greated or less than threshold.
                   pixel += offSet;
               }
            }
            copy.UnlockBits(bmd1);
            
            return copy;
        }
        private static int GetMean(Bitmap b) // returns mean pixel value in gray image
        {
            int sum = 0;
            
            int bW = b.Width, bH = b.Height;
            Bitmap copy = (Bitmap)ImageAlteration.GrayScale(b).Clone(); // Use clone with grayscale because it is changing original image
            /////// ^^^^^^^^^^^^^^^^ Grayscale conversion in both GetMean and ApplyThreshold, make it once.
            BitmapData bmd1 = copy.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

            int stride = bmd1.Stride;
            int offSet = stride - bW;

            unsafe
            { 
                byte* pixel = (byte*)bmd1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap.

                // Loop through the pixels.
                for (int y = 0; y < bH; y++)
                {
                    for (int x = 0; x < bW; x++, pixel++) 
                        sum += pixel[0]; // sums up pixel values
                    pixel += offSet;
                }
            }
            copy.UnlockBits(bmd1);
            
            return sum/(bW*bH); // returns mean
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        public static Bitmap AdaptiveThresholding4(Bitmap b, bool isMean)
        {
            int bW = b.Width, bH = b.Height, bH4 = bH/4, bH2 = bH/2;
            PixelFormat pF = PixelFormat.Format8bppIndexed;

            Bitmap newB = (Bitmap)b.Clone();
            BitmapData bmd = newB.LockBits(new Rectangle(0 ,0, bW, bH), ImageLockMode.ReadOnly, pF);
            Bitmap b1 = b.Clone(new Rectangle(0, 0, b.Width, bH4), pF);
            Bitmap b2 = b.Clone(new Rectangle(0, bH4, b.Width, bH4), pF);
            Bitmap b3 = b.Clone(new Rectangle(0, bH2, b.Width, bH4), pF);
            Bitmap b4 = b.Clone(new Rectangle(0, bH4*3, b.Width, bH4), pF);
            Bitmap[] bs = new[] { b1, b2, b3, b4 };
            int stride = bmd.Stride;


            unsafe
            {
                byte* start = (byte*)bmd.Scan0;
                for (int partition = 0; partition < 4; partition++)
                {
                    int t;
                    // if (isOtsu)
                    //     t = GetOtsu(bs[partition]);
                    // else
                    t = GetMean(bs[partition]);
                    for (int y = 0; y < bH4; y++)
                    {
                        byte* row = start + (y * stride);
                        for (int x = 0; x < bW; x++)
                        {
                            byte* pixel = row + x;
                            pixel[0] = (byte)(pixel[0] > t ? 255 : 0);
                        }
                    }
                    start += bH4*stride;
                }
                
            }
            
            // for (int i = 0; i < bs.Length; i++)
            // {
            //     int t = GetOtsu(bs[i]);
            //     bs[i] = ApplyThreshold(bs[i], t);
            // }

            return newB;
        }
        
        public static Bitmap AdaptiveThresholding8(Bitmap b, bool isMean)
        {
            int bW = b.Width, bH = b.Height, bH8 = bH/8, bH4 = bH/4, bH2 = bH/2;
            PixelFormat pF = PixelFormat.Format8bppIndexed;

            Bitmap newB = (Bitmap)b.Clone();
            BitmapData bmd = newB.LockBits(new Rectangle(0 ,0, bW, bH), ImageLockMode.ReadOnly, pF);
            Bitmap b1 = b.Clone(new Rectangle(0, 0, b.Width, bH8), pF);
            Bitmap b2 = b.Clone(new Rectangle(0, bH8, b.Width, bH8), pF);
            Bitmap b3 = b.Clone(new Rectangle(0, bH4, b.Width, bH8), pF);
            Bitmap b4 = b.Clone(new Rectangle(0, bH8*3, b.Width, bH8), pF);
            Bitmap b5 = b.Clone(new Rectangle(0, bH2, b.Width, bH8), pF);
            Bitmap b6 = b.Clone(new Rectangle(0, bH8*5, b.Width, bH8), pF);
            Bitmap b7 = b.Clone(new Rectangle(0, bH4*3, b.Width, bH8), pF);
            Bitmap b8 = b.Clone(new Rectangle(0, bH8*7, b.Width, bH8), pF);
            Bitmap[] bs = new[] { b1, b2, b3, b4, b5, b6, b7, b8 };
            int stride = bmd.Stride;


            unsafe
            {
                byte* start = (byte*)bmd.Scan0;
                for (int partition = 0; partition < 8; partition++)
                {
                    int t;
                    // if (isOtsu)
                    //     t = GetOtsu(bs[partition]);
                    // else
                    t = GetMean(bs[partition]);
                    for (int y = 0; y < bH8; y++)
                    {
                        byte* row = start + (y * stride);
                        for (int x = 0; x < bW; x++)
                        {
                            byte* pixel = row + x;
                            pixel[0] = (byte)(pixel[0] > t ? 255 : 0);
                        }
                    }
                    start += bH8*stride;
                }
                
            }
            
            // for (int i = 0; i < bs.Length; i++)
            // {
            //     int t = GetOtsu(bs[i]);
            //     bs[i] = ApplyThreshold(bs[i], t);
            // }

            return newB;
        }
    }
}