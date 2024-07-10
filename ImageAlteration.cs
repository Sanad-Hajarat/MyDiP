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
            if (b.PixelFormat == PixelFormat.Format8bppIndexed) // return original image if already 8bpp
                return (Bitmap)b.Clone();
                        
            int bW = b.Width, bH = b.Height;                    // initialize variables & puxel formats instead of repetitively calling them
            Bitmap grayBitmap = new Bitmap(bW, bH, PixelFormat.Format8bppIndexed);             // This bitmap will hold the gray image we will return
            BitmapData coloredLock = b.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.ReadOnly, b.PixelFormat);        // Read only info from original image
            BitmapData grayLock = grayBitmap.LockBits(new Rectangle(0, 0, bW, bH), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);  // Write on new 8-Bit image

            int pFsize = Image.GetPixelFormatSize(b.PixelFormat);  // returns 24 for 24bppRgb and 8 for 8bppIndexed etc..
            int bpp = pFsize / 8 > 1 ? pFsize / 8 : 1;  // stores int value in bpp that tells us how many bytes per pixel
                                                        //    ( < 1 =  1 bit )    (   3 = 24 bit )    (   4 = 32 bit )
            int strideColored = coloredLock.Stride;

            int offSetColored = strideColored - bW * bpp;   // offset is based on the bytes per pixel for original image
            int offSetGray = grayLock.Stride - bW;                // Grayscale so offset is determined for one channel

            ColorPalette palette = grayBitmap.Palette;
            for (int i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            grayBitmap.Palette = palette;             // define color palette for grayscale image.

            unsafe
            {
                byte* coloredPixel = (byte*)coloredLock.Scan0.ToPointer(); // gets a pointer to the first pixel data in the original image.
                byte* grayPixel = (byte*)grayLock.Scan0.ToPointer(); // gets a pointer to the first pixel data in the gray image.
                
                if (bpp <= 1) // if pf = 1bppIndexed
                {
                    for (int y = 0; y < bH; y++, coloredPixel += strideColored, grayPixel += offSetGray)
                    {
                        for (int x = 0; x < bW; x++, grayPixel++)    // increment x for trace and ptr2 to get to next pixel.
                        {
                            // byte pixelByte = *(coloredPixel + (x / 8));// finds byte responsible for pixel value e.x. (1000 0101)
                            int pixelValue = (*(coloredPixel + (x / 8)) >> (7 - (x % 8))) & 0x01;   // results in 0 or 1 only which is pixel value from bit.
                            *grayPixel |= (byte)(pixelValue > 0 ? 255 : 0);            // sets new gray image value to 255 or 0.
                        }
                    }
                }
                else    //if not 1 bit
                {
                    for (int y = 0; y < bH; y++)
                    {
                        for (int x = 0; x < bW; x++, coloredPixel += bpp, grayPixel++)   // increments gray image and non-gray image according to its bytes per pixel.
                            *grayPixel = (byte)(.299 * coloredPixel[2] + .587 * coloredPixel[1] + .114 * coloredPixel[0]); // calculates pixel color value
                        coloredPixel += offSetColored;
                        grayPixel += offSetGray;
                    }
                }
            }

            b.UnlockBits(coloredLock);
            grayBitmap.UnlockBits(grayLock);
            
            return grayBitmap;
        }

        public static Bitmap AddBorder(Bitmap b, byte color, int borderWidth)    // Adds border of certain color. Helps with dilation and erosion.
        {
            int bitmapWidth = b.Width, bitmapHeight = b.Height, bW2 = bitmapWidth + (borderWidth*2), bH2 = bitmapHeight + (borderWidth*2); // define new width and height for bordered image.
            Bitmap greyB = GrayScale(b);                                        // grayscale transformation to make it easier to deal with.
            Bitmap newB = new Bitmap(bW2, bH2, PixelFormat.Format8bppIndexed);
            BitmapData b1 = greyB.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);     
            BitmapData b2 = newB.LockBits(new Rectangle(0, 0, bW2, bH2),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);  
            
            unsafe
            {
                byte* normal = (byte*)b1.Scan0.ToPointer(); // gets a pointer to the first pixel data in the normal image.
                byte* padded = (byte*)b2.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bordered image.

                int offSet1 = b1.Stride - bitmapWidth, offSet2 = b2.Stride - bW2;
                
                for (int y = 0; y < bH2; y++, padded += offSet2)
                {
                    for (int x = 0; x < bW2; x++, padded++)
                    {
                        if (x < borderWidth || y < borderWidth || x > bitmapWidth + (borderWidth-1) || y > bitmapHeight + (borderWidth-1)) 
                            { *padded = color; }                                    // if on border make pixel = color
                        else { *padded = *normal; normal++; }                       // else pixel = normal pixel
                    }
                    if (y == 0)
                        continue;
                    normal += offSet1; // don't add offset if on first row
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
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), 
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed); // working on same image and calculating pixels to be removed

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
                        if (*ptrTop < 255 || *ptrBot < 255) // if either top or bot pixels not white
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
                        if (*ptrLeft < 255 || *ptrRight < 255) // if either left or right pixels not white
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

        public static Bitmap Rescale(Bitmap b, double factor) // Changes image resolution by resizing/rescaling to a factor
        {
            if (factor == 1)
                return (Bitmap)b.Clone();
            int originalWidth = b.Width, originalHeight = b.Height;
            int rescaledW = (int)(originalWidth * factor);
            int rescaledH = (int)(originalHeight * factor);  // using factor to change bW and bH.
            Bitmap copy = GrayScale(b);
            Bitmap newB = new Bitmap(rescaledW, rescaledH, PixelFormat.Format8bppIndexed);
            newB.Palette = copy.Palette;
            BitmapData original = copy.LockBits(new Rectangle(0, 0, originalWidth, originalHeight), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
            BitmapData rescaled = newB.LockBits(new Rectangle(0, 0, rescaledW, rescaledH), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

            int strideOriginal = original.Stride;
            int offSetRescaled = rescaled.Stride - rescaledW;

            unsafe
            {
                byte* originalPixel = (byte*)original.Scan0.ToPointer();  
                byte* rescaledPixel = (byte*)rescaled.Scan0.ToPointer();  

                for (int y = 0; y < rescaledH; y++, rescaledPixel += offSetRescaled) 
                {
                    int pixelY = (int) (y/factor);
                    byte* rowPtr = originalPixel + (pixelY * strideOriginal);

                    for (int x = 0; x < rescaledW; x++, rescaledPixel++)
                    {
                        int pixelX = (int)(x / factor);
                        *rescaledPixel = *(rowPtr + pixelX);
                    }
                }
            }
            
            copy.UnlockBits(original);
            newB.UnlockBits(rescaled);
            
            return newB;
        }
    } 
}