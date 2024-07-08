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
        public static Bitmap GrayScale(Bitmap b)  // Image's are converted to 8-Bit Per Pixel Grayscale
        {
            PixelFormat pF = b.PixelFormat;
            if (pF == PixelFormat.Format8bppIndexed) // return original image if already 8bpp
                return b;
                        
            int bW = b.Width, bH = b.Height;                  // initialize variables & puxel formats instead of repetitively calling them
            PixelFormat bit8 = PixelFormat.Format8bppIndexed;
            
            Bitmap newB = new Bitmap(bW, bH, bit8);             // This bitmap will hold the gray image we will return
            BitmapData b1 = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF);        // Read only info from original image
            BitmapData b2 = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadWrite, bit8);  // Write on new 8-Bit image

            int pFsize = Image.GetPixelFormatSize(pF);  // returns 24 for 24bppRgb and 8 for 8bppIndexed etc..
            int bpp = pFsize / 8 > 1 ? pFsize / 8 : 1;  // stores int value in bpp that tells us how many bytes per pixel ( < 1 =  1 bit )
            bool is1Bit = bpp <= 1;                     //                                                                (   3 = 24 bit )
                                                        //                                                                (   4 = 32 bit )
            int stride1 = b1.Stride;
            int stride2 = b2.Stride;

            int offSet1 = stride1 - bW * bpp; // offset is based on the bytes per pixel for original image
            int offSet2 = stride2 - bW; // Grayscale so offset is determined easily

            ColorPalette palette = newB.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            newB.Palette = palette;             // define color palette for grayscale image.

            unsafe
            {
                byte* ptr1 = (byte*)b1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the original image.
                byte* ptr2 = (byte*)b2.Scan0.ToPointer(); // gets a pointer to the first pixel data in the gray image.
                
                if (is1Bit) // if 1bpp
                {
                    for (int y = 0; y < bH; y++)
                    {
                        byte* rowBit = ptr1 + (y * stride1);    // ptr tracks start of each row.
                        for (int x = 0; x < bW; x++, ptr2++)    // increment x for trace and ptr2 to get to next pixel.
                        {
                            byte pixelByte = *(rowBit + (x / 8));// finds byte responsible for pixel value e.x. (1000 0101)
                            int pixelValue = (pixelByte >> (7 - (x % 8))) & 0x01;   // results in 0 or 1 only which is pixel value from bit.
                            ptr2[0] |= (byte)(pixelValue > 0 ? 255 : 0);            // sets new gray image value to 255 or 0.
                        }
                        ptr2 += offSet2;
                    }
                }
                else    //if not 1 bit
                {
                    for (int y = 0; y < bH; y++)
                    {
                        for (int x = 0; x < bW; x++, ptr1 += bpp, ptr2++)   // increments gray image and non-gray image according to its bytes per pixel.
                            ptr2[0] = (byte)(.299 * ptr1[2] + .587 * ptr1[1] + .114 * ptr1[0]); // calculates pixel color value
                        ptr1 += offSet1;
                        ptr2 += offSet2;
                    }
                }
            }

            b.UnlockBits(b1);
            newB.UnlockBits(b2);
            
            return newB;
        }

        public static Bitmap AddBorder(Bitmap b, byte color)    // Adds border of certain color. Helps with dilation and erosion.
        {
            int bW = b.Width, bH = b.Height, bW2 = bW + 2, bH2 = bH + 2; // define new width and height for bordered image.
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            Bitmap greyB = (Bitmap)ImageAlteration.GrayScale(b).Clone();                 // grayscale transformation to make it easier to deal with.
            Bitmap newB = new Bitmap(bW2, bH2, pF);
            BitmapData b1 = greyB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF);     
            BitmapData b2 = newB.LockBits(new Rectangle(0, 0, bW2, bH2), ImageLockMode.ReadWrite, pF);  
            
            unsafe
            {
                byte* pixel1 = (byte*)b1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the normal image.
                byte* pixel2 = (byte*)b2.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bordered image.

                int stride1 = b1.Stride;
                int stride2 = b2.Stride;

                int offSet1 = stride1 - bW, offSet2 = stride2 - bW2;
                
                for (int y = 0; y < bH2; y++)
                {
                    for (int x = 0; x < bW2; x++, pixel2++)
                    {
                        if (x < 1 || y < 1 || x > bW || y > bH) { pixel2[0] = color; }  // if on border make pixel = color
                        else { pixel2[0] = pixel1[0]; pixel1++; }                       // else              pixel = normal pixel
                    }
                    pixel2 += offSet2;
                    if (y == 0)
                        continue;
                    pixel1 += offSet1; // don't add offset if on first row
                }
            }
            greyB.UnlockBits(b1);
            newB.UnlockBits(b2);
            
            return newB;
        }
        
        public static Bitmap RemoveWhiteBounds(Bitmap b) // Remove's white boundary from image in an equal manner.
        {
            Bitmap newB = Binarization.ApplyStaticThreshold(b, 200); // Applies grayscale and high threshold to define pure white from shadow
            int bW = b.Width, bH = b.Height;
            PixelFormat pF = PixelFormat.Format8bppIndexed;
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, pF); // working on same image and calculating pixels to be removed

            bool breakIt = false;       // boolean determines if we need to break search because non white border found.
            int skip = 0;               // how many pixels we are skipping from final image because of white border.
            int stride = bmd.Stride;    // NON-CHANGING stride value.
            int last = bW - 2;          // used for calculating pixel on right side (last stands for last pixel in row).
            
            unsafe
            {
                while (true) // keep incrementing until non-white border found.
                {   
                    byte* ptrTop = (byte*)bmd.Scan0.ToPointer() + (stride*skip) + skip;         // First pixel from top-left corner of image
                    byte* ptrBot = ptrTop + (stride * (bH - 1));                                // Starting pixel from last row
                    byte* ptrLeft = ptrTop + stride;                                            // One row after start pixel to not compare corner pixels twice
                    byte* ptrRight = ptrLeft + last;                                            // Complete opposite side of ptrLeft

                    for (int i = 0; i < bW; i++)
                    {
                        if (ptrTop[0] < 255 || ptrBot[0] < 255) // if either top or bot pixels not white
                        { 
                            breakIt = true; 
                            break;
                        }
                        ptrTop++;
                        ptrBot++;
                    }

                    if (breakIt) { break; }

                    for (int i = 0; i < bH-2; i++) // < bH by 2 to not compare corners twice
                    {
                        if (ptrLeft[0] < 255 || ptrRight[0] < 255) // if either left or right pixels not white
                        { 
                            breakIt = true; 
                            break;
                        }
                        ptrLeft += stride;
                        ptrRight += stride;
                    }
                    
                    if (breakIt) { break; }
                    
                    skip++;         // for top & left ptrs calculation
                    last -= 2;      // for right (left is incremented)

                    bH -= 2;        //  image width is decreasing
                    bW -= 2;        // image height is decreasing
                }
            }
            newB.UnlockBits(bmd);


            return b.Clone(new Rectangle(skip, skip, bW, bH), b.PixelFormat);   // skip is used to trace where the white boundaries are repeating.
        } // Returns same Image cropped by exactly n rows and columns equally

        public static Bitmap Rescale(Bitmap b, double factor) // Changes image resolution by resizing/rescaling
        {
            if (factor == 1)
                return b;
            int newW = (int)(b.Width * factor), newH = (int)(b.Height * factor);    // using factor to change bW and bH.
            Bitmap newB = new Bitmap(b, new Size(newW, newH));                      // using built in constructor to rescale image.
            return newB;
        }
    } 
}