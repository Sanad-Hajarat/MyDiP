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
                            byte pixelByte = *(coloredPixel + (x / 8));// finds byte responsible for pixel value e.x. (1000 0101)
                            int pixelValue = (pixelByte >> (7 - (x % 8))) & 0x01;   // results in 0 or 1 only which is pixel value from bit.
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
            BitmapData grayImage = greyB.LockBits(new Rectangle(0, 0, bitmapWidth, bitmapHeight),
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);     
            BitmapData paddedImage = newB.LockBits(new Rectangle(0, 0, bW2, bH2),
                ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);  

            unsafe
            {
                byte* normal = (byte*)grayImage.Scan0.ToPointer(); // gets a pointer to the first pixel data in the normal image.
                byte* padded = (byte*)paddedImage.Scan0.ToPointer(); // gets a pointer to the first pixel data in the bordered image.

                int offSet1 = grayImage.Stride - bitmapWidth, offSet2 = paddedImage.Stride - bW2;
                
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
            greyB.UnlockBits(grayImage);
            newB.UnlockBits(paddedImage);

            return newB;
        }

        public static Bitmap RemoveWhiteBounds(Bitmap b) // Remove's white boundary from image in an equal manner.
        {
            Bitmap newB = Binarization.ApplyStaticThreshold(b, 200); // Applies grayscale and high threshold to define pure white from shadow
            int bW = b.Width, bH = b.Height;
            BitmapData bmd = newB.LockBits(new Rectangle(0, 0, bW, bH), 
                ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed); // working on same image and calculating pixels to be removed

            bool breakTop   = false;      // boolean determines if we need to break search because non white border found.
            bool breakLeft  = false;
            bool breakRight = false;
            bool breakBot   = false;

            int skipTop   = 0;         // how many pixels we are skipping from final image because of white border.
            int skipLeft  = 0;
            int skipRight = 0;
            int skipBot   = 0;
            int stride = bmd.Stride;    // NON-CHANGING stride value.

            int last = bW - 1;          // used for calculating pixel on right side (last stands for last pixel in row).
            
            unsafe
            {  
                byte* ptr = (byte*)bmd.Scan0.ToPointer();                        // First pixel from top-left corner of image
                while (true)
                {
                    byte* ptrTop = ptr + stride*skipTop;
                    for (int i = 0; i < bW; i++)
                    {
                        if (*ptrTop < 255) // if either top or bot pixels not white
                        { 
                            breakTop = true;
                            break;
                        }
                        ptrTop++;
                    }
                    if (breakTop)
                        break;

                    skipTop++;
                }

                while (true)
                {
                    byte* ptrLeft = ptr + stride*skipTop + skipLeft;
                    for (int i = 0; i < bH - skipTop; i++)
                    {
                        if (*ptrLeft < 255) // if either top or bot pixels not white
                        { 
                            breakLeft = true;
                            break;
                        }
                        ptrLeft += stride;
                    }
                    if (breakLeft)
                        break;

                    skipLeft++;
                }

                while (true)
                {
                    byte* ptrRight = ptr + stride*skipTop + bW - 1 - skipRight;
                    for (int i = 0; i < bH - skipTop; i++)
                    {
                        if (*ptrRight < 255) // if either top or bot pixels not white
                        { 
                            breakRight = true;
                            break;
                        }
                        ptrRight += stride;
                    }
                    if (breakRight)
                        break;

                    skipRight++;
                }

                while (true)
                {
                    byte* ptrBot = ptr + stride*(bH-1-skipBot) + skipLeft;
                    for (int i = 0; i < bW - skipRight - skipLeft; i++)
                    {
                        if (*ptrBot < 255) // if either top or bot pixels not white
                        { 
                            breakBot = true;
                            break;
                        }
                        ptrBot++;
                    }
                    if (breakBot)
                        break;

                    skipBot++;
                }
            }

            newB.UnlockBits(bmd);


            return b.Clone(new Rectangle(skipLeft, skipTop, bW-skipRight-skipLeft, bH-skipBot-skipTop), b.PixelFormat);   

        }   // Returns same Image cropped by exactly n rows and columns equally

        public static Bitmap Rescale(Bitmap b, double factor) // Changes image resolution by resizing/rescaling
        {
            Bitmap copy = (Bitmap)b.Clone();
            copy.Palette = b.Palette;
            if (factor == 1)
                return copy;
            if ((copy.PixelFormat != PixelFormat.Format8bppIndexed && copy.PixelFormat != PixelFormat.Format24bppRgb))
                copy = GrayScale(b);
            
            int originalWidth = copy.Width, originalHeight = copy.Height;
            int rescaledW = (int)(originalWidth * factor);
            int rescaledH = (int)(originalHeight * factor);  // using factor to change bW and bH.

            Bitmap newB = new Bitmap(rescaledW, rescaledH, copy.PixelFormat);
            newB.Palette = b.Palette;

            BitmapData original = copy.LockBits(new Rectangle(0, 0, originalWidth, originalHeight), ImageLockMode.ReadOnly, copy.PixelFormat);
            BitmapData rescaled = newB.LockBits(new Rectangle(0, 0, rescaledW, rescaledH), ImageLockMode.WriteOnly, copy.PixelFormat);

            int strideOriginal = original.Stride;
            int bpp = Image.GetPixelFormatSize(copy.PixelFormat) / 8;
            int offSetRescaled = rescaled.Stride - rescaledW * bpp;

            unsafe
            {
                byte* originalPixel = (byte*)original.Scan0.ToPointer();  
                byte* rescaledPixel = (byte*)rescaled.Scan0.ToPointer();  

                for (int y = 0; y < rescaledH; y++) 
                {
                    int pixelY = (int) (y / factor);
                    byte* rowPtr = originalPixel + (pixelY * strideOriginal);

                    for (int x = 0; x < rescaledW * bpp; x+= bpp, rescaledPixel+= bpp)
                    {
                        int pixelX = (int)(x / factor);
                        for (int i = 0; i < bpp; i++)
                            rescaledPixel[i] = (rowPtr + pixelX)[i];
                    }
                    rescaledPixel += offSetRescaled;
                }
            }
            
            copy.UnlockBits(original);
            newB.UnlockBits(rescaled);
            
            return newB;
        }
    } 
}