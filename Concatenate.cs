using System.Drawing;
using System.Drawing.Imaging;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    
    // Concatenation, returns 2 images either vertically or horizontally concatenated
    public class Concatenate 
    {
        public static Bitmap Horizontal(Bitmap b1, Bitmap b2)
        {
            Bitmap bm1 = ImageAlteration.GrayScale(b1), bm2 = ImageAlteration.GrayScale(b2);
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            int w1 = b1.Width, h1 = b1.Height, w2 = b2.Width, h2 = b2.Height, bigW = w1 + w2, bigH = h1 >= h2 ? h1 : h2;
            
            BitmapData bmd1 = bm1.LockBits(new Rectangle(0, 0, w1, h1), ImageLockMode.ReadOnly, pF);
            BitmapData bmd2 = bm2.LockBits(new Rectangle(0, 0, w2, h2), ImageLockMode.ReadOnly, pF);

            Bitmap concat = new Bitmap(bigW, bigH, pF);
            concat.Palette = bm1.Palette;
            
            BitmapData bmdC = concat.LockBits(new Rectangle(0, 0, bigW, bigH), ImageLockMode.ReadWrite, pF);

            int stride1 = bmd1.Stride;
            int stride2 = bmd2.Stride;
            int bigStride = bmdC.Stride;

            int offSet1 = stride1 - w1;
            int offSet2 = stride2 - w2;
            int offSet = bigStride - bigW;

            unsafe
            {
                byte* ptr1 = (byte*)bmd1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap1.
                byte* ptr2 = (byte*)bmd2.Scan0.ToPointer();
                byte* pixel = (byte*)bmdC.Scan0.ToPointer();    
                // byte* POINTER = (byte*)bmdC.Scan0;
                
                for (int i = 0; i < bigH; i++)
                {
                    // byte* ROW = POINTER + (i * bigStride); USING OFFSET INSTEAD OF THIS
                    for (int j = 0; j < w1; j++, pixel++) 
                    {
                        if (i >= h1)
                            pixel[0] = 255;
                        else
                        {
                            // PIXEL = (ROW + j);
                            pixel[0] = ptr1[0];
                            ptr1++;
                        }

                    }

                    for (int j = 0; j < w2; j++, pixel++)
                    {
                        if (i >= h2)
                            pixel[0] = 255;
                        else
                        {
                            pixel[0] = ptr2[0];
                            ptr2++;
                        }
                    }
                    
                    pixel += offSet;
                    ptr1 += offSet1;
                    ptr2 += offSet2;
                }
            }
            bm1.UnlockBits(bmd1);
            bm2.UnlockBits(bmd2);
            concat.UnlockBits(bmdC);
            
            return concat;
        }

        public static Bitmap Vertical(Bitmap b1, Bitmap b2)
        {
            Bitmap bm1 = ImageAlteration.GrayScale(b1), bm2 = ImageAlteration.GrayScale(b2);
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            int w1 = b1.Width, h1 = b1.Height, w2 = b2.Width, h2 = b2.Height, bigW = w1 >= w2 ? w1 : w2, bigH = h1 + h2;
            
            BitmapData bmd1 = bm1.LockBits(new Rectangle(0, 0, w1, h1), ImageLockMode.ReadOnly, pF);
            BitmapData bmd2 = bm2.LockBits(new Rectangle(0, 0, w2, h2), ImageLockMode.ReadOnly, pF);

            Bitmap concat = new Bitmap(bigW, bigH, pF);
            concat.Palette = bm1.Palette;
            
            BitmapData bmdC = concat.LockBits(new Rectangle(0, 0, bigW, bigH), ImageLockMode.ReadWrite, pF);

            int stride1 = bmd1.Stride;
            int stride2 = bmd2.Stride;
            int bigStride = bmdC.Stride;

            int offSet1 = stride1 - w1;
            int offSet2 = stride2 - w2;
            int offSet = bigStride - bigW;

            unsafe
            {
                byte* ptr1 = (byte*)bmd1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap1.
                byte* ptr2 = (byte*)bmd2.Scan0.ToPointer();
                byte* pixel = (byte*)bmdC.Scan0.ToPointer();
                
                for (int i = 0; i < h1; i++)
                {
                    // byte* row1 = ptr1 + (i * stride1);
                    for (int j = 0; j < bigW; j++, pixel++)
                    {
                        if (j >= w1)
                            pixel[0] = 255;
                        else
                        {
                            // pixel[0] = (row1 + j)[0];
                            pixel[0] = ptr1[0];
                            ptr1++;
                        }
                    }
                    pixel += offSet;
                    ptr1 += offSet1;
                }

                for (int i = 0; i < h2; i++)
                {
                    // byte* row2 = ptr2 + (i * stride2);
                    for (int j = 0; j < bigW; j++, pixel++)
                    {
                        if (j >= w2)
                            pixel[0] = 255;
                        else
                        {
                            // pixel[0] = (row2 + j)[0];
                            pixel[0] = ptr2[0];
                            ptr2++;
                        }
                    }
                    pixel += offSet;
                    ptr2 += offSet2;
                }
                
            }
            bm1.UnlockBits(bmd1);
            bm2.UnlockBits(bmd2);
            concat.UnlockBits(bmdC);
            
            return concat;
        }
    }
}