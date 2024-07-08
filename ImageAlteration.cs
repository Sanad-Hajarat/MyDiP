using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace SanadDiP
{
    /// <CREDITS>
    /// Sanad Hajarat
    /// Data Science Intern
    /// ProgressSoft Corporation
    /// </CREDITS>
    
    // Simply a class used to alter images, either applying grayscale or adding and removing borders.
    public class ImageAlteration
    {
        public static Bitmap GrayScale(Bitmap b)
        {
            PixelFormat pF = b.PixelFormat;
            if (pF == PixelFormat.Format8bppIndexed)
                return b;
            
            // Console.WriteLine("Converting");
            
            int bW = b.Width, bH = b.Height;
            PixelFormat bit8 = PixelFormat.Format8bppIndexed;
            
            Bitmap newB = new Bitmap(bW, bH, bit8);
            BitmapData b1 = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF);
            BitmapData b2 = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, bit8);

            int pFsize = Image.GetPixelFormatSize(pF);
            int bpp = pFsize / 8 > 1 ? pFsize / 8 : 1;
            bool is1Bit = bpp <= 1;

            int stride1 = b1.Stride;
            int stride2 = b2.Stride;

            int offSet1 = stride1 - bW * bpp; // offset is based on the bytes per pixel
            int offSet2 = stride2 - bW; // Grayscale so offset is determined easily

            ColorPalette palette = newB.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            newB.Palette = palette;

            unsafe
            {
                byte* ptr1 = (byte*)b1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap.
                byte* ptr2 = (byte*)b2.Scan0.ToPointer(); 
                
                if (is1Bit)
                {
                    for (int y = 0; y < bH; y++)
                    {
                        byte* rowBit = ptr1 + (y * stride1);
                        for (int x = 0; x < bW; x++, ptr2++)
                        {
                            byte pixelBit = *(rowBit + (x / 8));
                            int pixelValue = (pixelBit >> (7 - (x % 8))) & 0x01; // results in 0 or 1 only
                            ptr2[0] |= (byte)(pixelValue > 0 ? 255 : 0);
                        }
                        ptr2 += offSet2;
                    }
                }
                else
                {
                    for (int y = 0; y < bH; y++)
                    {
                        for (int x = 0; x < bW; x++, ptr1 += bpp, ptr2++)
                            ptr2[0] = (byte)(.299 * ptr1[2] + .587 * ptr1[1] + .114 * ptr1[0]);
                        ptr1 += offSet1;
                        ptr2 += offSet2;
                    }
                }
            }

            b.UnlockBits(b1);
            newB.UnlockBits(b2);
            
            return newB;
        }

        public static Bitmap AddBorder(Bitmap b, byte color)
        {
            int bW = b.Width, bH = b.Height, bW2 = bW + 2, bH2 = bH + 2;
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            Bitmap greyB = ImageAlteration.GrayScale(b);
            Bitmap newB = new Bitmap(bW2, bH2, pF);
            BitmapData b1 = greyB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF);
            BitmapData b2 = newB.LockBits(new Rectangle(0, 0, bW2, bH2), ImageLockMode.ReadWrite, pF);
            
            unsafe
            {
                byte* pixel1 = (byte*)b1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bitmap.
                byte* pixel2 = (byte*)b2.Scan0.ToPointer();

                int stride1 = b1.Stride;
                int stride2 = b2.Stride;

                int offSet1 = stride1 - bW, offSet2 = stride2 - bW2;
                
                for (int y = 0; y < bH2; y++)
                {
                    for (int x = 0; x < bW2; x++, pixel2++)
                    {
                        if (x < 1 || y < 1 || x > bW || y > bH)
                            pixel2[0] = color;
                        else { pixel2[0] = pixel1[0]; pixel1++; }
                    }
                    pixel2 += offSet2;
                    if (y == 0)
                        continue;
                    pixel1 += offSet1;
                }
            }
            greyB.UnlockBits(b1);
            newB.UnlockBits(b2);
            
            return newB;
        }
        
        public static Bitmap RemoveWhiteBounds(Bitmap b)
        {
            Bitmap newB = Binarization.ApplyStaticThreshold(b, 200); // Applies grayscale and threshold to define pure white from shadow
            int bW = b.Width, bH = b.Height;
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF);

            bool breakIt = false;
            int skip = 0;
            int stride = bmd.Stride;
            int last = bW - 2;
            
            unsafe
            {
                while (true)
                {   
                    // duplicate comparisons on corners.
                    byte* ptrTop = (byte*)bmd.Scan0.ToPointer() + (stride*skip) + skip;
                    byte* ptrBot = ptrTop + (stride * (bH - 1)); //How to calculate last row?
                    byte* ptrLeft = ptrTop + stride; // no longer duplicate top left&right corners
                    byte* ptrRight = ptrLeft + last; 

                    for (int i = 0; i < bW; i++)
                    {
                        if (ptrTop[0] < 255 || ptrBot[0] < 255)
                        { 
                            // Console.WriteLine($"Try{skip} pixel{i} T: {ptrTop[0]}, B: {ptrBot[0]}");
                            breakIt = true; 
                            break;
                        }
                        ptrTop++;
                        ptrBot++;
                    }

                    if (breakIt) { break; }

                    for (int i = 0; i < bH-2; i++)
                    {
                        if (ptrLeft[0] < 255 || ptrRight[0] < 255)
                        { 
                            // Console.WriteLine($"Try{skip} pixel{i} L: {ptrLeft[0]}, R: {ptrRight[0]}");
                            breakIt = true; 
                            break;
                        }
                        ptrLeft += stride;
                        ptrRight += stride;
                    }
                    
                    if (breakIt) { break; }
                    
                    skip++; // for top & left
                    last -= 2; // for right (left is incremented)

                    bH -= 2;
                    bW -= 2;
                }
            }
            newB.UnlockBits(bmd);


            return b.Clone(new Rectangle(skip, skip, bW, bH), b.PixelFormat);
        } // Returns same Image cropped by exactly n rows and columns equally

        public static Bitmap Rescale(Bitmap b, double factor)
        {
            if (factor == 1)
                return b;
            int newW = (int)(b.Width * factor), newH = (int)(b.Height * factor);
            Bitmap newB = new Bitmap(b, new Size(newW, newH));
            return newB;
        }
    } 
}