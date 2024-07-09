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
        public static Bitmap Horizontal(Bitmap b1, Bitmap b2) // Horizontal concatenation means width = W1 + W2, height = Max(H1, H2)
        {
            Bitmap bm1 = ImageAlteration.GrayScale(b1), bm2 = ImageAlteration.GrayScale(b2); // convert both grayscale if not already
            PixelFormat pF = PixelFormat.Format8bppIndexed; //////// const
            int w1 = b1.Width, h1 = b1.Height, w2 = b2.Width, h2 = b2.Height, bigW = w1 + w2, bigH = h1 >= h2 ? h1 : h2;
            
            BitmapData bmd1 = bm1.LockBits(new Rectangle(0, 0, w1, h1), ImageLockMode.ReadOnly, pF);
            BitmapData bmd2 = bm2.LockBits(new Rectangle(0, 0, w2, h2), ImageLockMode.ReadOnly, pF);

            Bitmap concat = new Bitmap(bigW, bigH, pF);     // create bitmap of new size.
            concat.Palette = bm1.Palette;                   // alter new bitmap palette
            
            BitmapData bmdC = concat.LockBits(new Rectangle(0, 0, bigW, bigH), ImageLockMode.ReadWrite, pF);

            int stride1 = bmd1.Stride;
            int stride2 = bmd2.Stride;
            int bigStride = bmdC.Stride;

            int offSet1 = stride1 - w1;
            int offSet2 = stride2 - w2;
            int offSet = bigStride - bigW;

            unsafe
            {
                byte* ptr1 = (byte*)bmd1.Scan0.ToPointer();     // gets a pointer to the first pixel data in the first image.
                byte* ptr2 = (byte*)bmd2.Scan0.ToPointer();     // gets a pointer to the first pixel data in the second image.
                byte* pixel = (byte*)bmdC.Scan0.ToPointer();    // gets a pointer to the first pixel data in the concattenated image.
                
                for (int i = 0; i < bigH; i++) // looping through whole image height
                {

                    for (int j = 0; j < w1; j++, pixel++)  // first image copying row values
                    {
                        if (i >= h1)
                            pixel[0] = 255; // if image height is shorter than whole concattenated image height make it white pixel
                        else
                        {
                            pixel[0] = ptr1[0]; // copy first image pixel
                            ptr1++;
                        }

                    }

                    for (int j = 0; j < w2; j++, pixel++) // second image copying row values
                    {
                        if (i >= h2)
                            pixel[0] = 255; // if image height is shorter than whole concattenated image height make it white pixel
                        else
                        {
                            pixel[0] = ptr2[0]; // copy second image pixel
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

        public static Bitmap Vertical(Bitmap b1, Bitmap b2) // Vertical concatenation means width = Max(W1, W2), height = H1 + H2
        {
            Bitmap bm1 = ImageAlteration.GrayScale(b1), bm2 = ImageAlteration.GrayScale(b2); // convert both grayscale if not already
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            int w1 = b1.Width, h1 = b1.Height, w2 = b2.Width, h2 = b2.Height, bigW = w1 >= w2 ? w1 : w2, bigH = h1 + h2;
            
            BitmapData bmd1 = bm1.LockBits(new Rectangle(0, 0, w1, h1), ImageLockMode.ReadOnly, pF);
            BitmapData bmd2 = bm2.LockBits(new Rectangle(0, 0, w2, h2), ImageLockMode.ReadOnly, pF);

            Bitmap concat = new Bitmap(bigW, bigH, pF); // create bitmap of new size.
            concat.Palette = bm1.Palette;               // alter new bitmap palette
            
            BitmapData bmdC = concat.LockBits(new Rectangle(0, 0, bigW, bigH), ImageLockMode.ReadWrite, pF);

            int stride1 = bmd1.Stride;
            int stride2 = bmd2.Stride;
            int bigStride = bmdC.Stride;

            int offSet1 = stride1 - w1;
            int offSet2 = stride2 - w2;
            int offSet = bigStride - bigW;

            unsafe
            {
                byte* ptr1 = (byte*)bmd1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the first image.
                byte* ptr2 = (byte*)bmd2.Scan0.ToPointer(); // gets a pointer to the first pixel data in the second image.
                byte* pixel = (byte*)bmdC.Scan0.ToPointer();// gets a pointer to the first pixel data in the concatenated image.
                
                for (int i = 0; i < h1; i++)    // copy whole first image into concatenated image
                {
                    for (int j = 0; j < bigW; j++, pixel++)
                    {
                        if (j >= w1)
                            pixel[0] = 255; // if image width is shorter than whole concattenated image width make it white pixel
                        else
                        {
                            pixel[0] = ptr1[0]; // copy first image pixel
                            ptr1++;
                        }
                    }
                    pixel += offSet;
                    ptr1 += offSet1;
                }

                for (int i = 0; i < h2; i++)
                {
                    for (int j = 0; j < bigW; j++, pixel++)
                    {
                        if (j >= w2)
                            pixel[0] = 255; // if image width is shorter than whole concattenated image width make it white pixel
                        else
                        {
                            pixel[0] = ptr2[0]; // copy second image pixel
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